using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Data.Google;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class MileageService : IMileageService
    {
        private readonly ILogger _logger;
        private readonly IConfigurationRoot _configuration;
        private readonly Uri _pcMilerAddress;
        private readonly string _pcMilerVersion;

        private readonly HttpClient _httpClient;
        private readonly string _googleDirectionApiUrl;
        private readonly string _googleKey;

        public MileageService(ILogger<MileageService> logger, IConfigurationRoot configuration, HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _pcMilerAddress = new Uri(_configuration["PcMilerAddress"]);
            _pcMilerVersion = _configuration["PcMilerVersion"] ?? "240";

            _httpClient = httpClient;
            _googleDirectionApiUrl = _configuration["GoogleDirectionsApiUrl"];
            _googleKey = _configuration["GoogleAPIKey"];
        }

        public int GetDirectMiles(MileageRequestData request)
        {
            if (request == null)
                return 0;

            try
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                };
                using (var httpClient = new HttpClient(handler))
                {
                    httpClient.BaseAddress = new Uri(string.Format("{0}://{1}", _pcMilerAddress.Scheme, _pcMilerAddress.Host));
                    httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                    httpClient.DefaultRequestHeaders.Add("SOAPAction", "\"http://tempuri.org/IPcMilerRest/GetMiles\"");
                    httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
                    httpClient.DefaultRequestHeaders.Add("Host", _pcMilerAddress.Host);

                    if (request.DestinationCountry == "CAN")
                        request.DestinationCountry = "CA";
                    if (request.OriginCountry == "CAN")
                        request.OriginCountry = "CA";

                    string xmlRequestBody = string.Format("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Body><GetMiles xmlns=\"http://tempuri.org/\"><version>" + _pcMilerVersion + "</version><origin xmlns:a=\"http://schemas.datacontract.org/2004/07/TOPSPCMiler.AppCode\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"><a:Addr1 i:nil=\"true\"/><a:Addr2 i:nil=\"true\"/><a:City{0}</a:City><a:Country>{2}</a:Country><a:IsValid>false</a:IsValid><a:LocationType>Origin</a:LocationType><a:LookupAddr i:nil=\"true\"/><a:PcMilerFormatted i:nil=\"true\"/><a:State{1}</a:State><a:Zip>{6}</a:Zip></origin><destination xmlns:a=\"http://schemas.datacontract.org/2004/07/TOPSPCMiler.AppCode\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"><a:Addr1 i:nil=\"true\"/><a:Addr2 i:nil=\"true\"/><a:City{3}</a:City><a:Country>{5}</a:Country><a:IsValid>false</a:IsValid><a:LocationType>Destination</a:LocationType><a:LookupAddr i:nil=\"true\"/><a:PcMilerFormatted i:nil=\"true\"/><a:State{4}</a:State><a:Zip>{7}</a:Zip></destination><config xmlns:a=\"http://schemas.datacontract.org/2004/07/TOPSPCMiler.AppCode\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"><a:ConfigId>DEFAULT</a:ConfigId><a:MilageCalcType>0</a:MilageCalcType><a:OpenBorders>true</a:OpenBorders><a:UseStreets>false</a:UseStreets></config></GetMiles></s:Body></s:Envelope>", string.IsNullOrEmpty(request.OriginCity) ? " i:nil=\"true\">" : ">" + request.OriginCity + "*", string.IsNullOrEmpty(request.OriginState) ? " i:nil=\"true\">" : ">" + request.OriginState, request.OriginCountry, string.IsNullOrEmpty(request.DestinationCity) ? " i:nil=\"true\">" : ">" + request.DestinationCity + "*", string.IsNullOrEmpty(request.DestinationState) ? " i:nil=\"true\">" : ">" + request.DestinationState, request.DestinationCountry, request.OriginPostalCode, request.DestinationPostalCode);

                    var messageResponse = httpClient.PostAsync(_pcMilerAddress.LocalPath, new StringContent(xmlRequestBody, System.Text.Encoding.UTF8, "text/xml")).Result;
                    string xmlResponse = messageResponse.Content.ReadAsStringAsync().Result;

                    System.Xml.XmlDocument xmlResponseDocument = new System.Xml.XmlDocument();
                    xmlResponseDocument.LoadXml(xmlResponse);

                    var hasErrors = xmlResponseDocument.GetElementsByTagName("a:Errors")[0].ChildNodes.Count > 0;
                    if (hasErrors)
                    {
                        _logger.LogInformation($"GetDirectMiles Error for request ({request.OriginCity}, {request.OriginState} {request.OriginCountry} {request.OriginPostalCode} to " +
                            $"{request.DestinationCity}, {request.DestinationState} {request.DestinationCountry} {request.DestinationPostalCode}): {xmlResponseDocument.GetElementsByTagName("a:Errors")[0].InnerText}");

                        if (xmlResponseDocument.GetElementsByTagName("a:Errors")[0].ChildNodes[0].InnerText.ToLower().Contains("invalid destination location")
                            && !string.IsNullOrEmpty(request.DestinationCity) && !string.IsNullOrEmpty(request.DestinationState))
                        {
                            //Try Destination with only zip code
                            request.DestinationCity = "";
                            request.DestinationState = "";
                            return GetDirectMiles(request);
                        }

                        if (xmlResponseDocument.GetElementsByTagName("a:Errors")[0].ChildNodes[0].InnerText.ToLower().Contains("invalid origin location")
                            && !string.IsNullOrEmpty(request.OriginCity) && !string.IsNullOrEmpty(request.OriginState))
                        {
                            //Try Origin with only zip code
                            request.OriginCity = "";
                            request.OriginState = "";
                            return GetDirectMiles(request);
                        }

                        return request.DefaultMiles;
                    }
                    else
                    {
                        string milesString = xmlResponseDocument.GetElementsByTagName("a:Miles")[0].InnerText;
                        decimal.TryParse(milesString, out decimal miles);
                        return Convert.ToInt32(miles);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"GetDirectMiles Error for request ({request.OriginCity}, {request.OriginState} {request.OriginCountry} {request.OriginPostalCode} to " +
                    $"{request.DestinationCity}, {request.DestinationState} {request.DestinationCountry} {request.DestinationPostalCode}): {e.Message}");
                return request.DefaultMiles;
            }
        }

        public async Task<int> GetDirectRouteMiles(MileageRequestData request)
        {
            if (request == null)
                return 0;

            var url = $"{_googleDirectionApiUrl}?" +
                $"origin={request.OriginCity}, {request.OriginState} {request.OriginPostalCode}" +
                $"&destination={request.DestinationCity}, {request.DestinationState} {request.DestinationPostalCode}" +
                $"&key={_googleKey}";
            var apiResponse = await _httpClient.GetAsync(url);
            apiResponse.EnsureSuccessStatusCode();

            var content = await apiResponse.Content.ReadAsStringAsync();
            var directions = JsonConvert.DeserializeObject<GoogleDirectionClass>(content);
            var meters = directions?.routes?.FirstOrDefault()?.legs?.FirstOrDefault()?.distance?.value;

            return (int)Math.Round((meters ?? 0) / 1609.34, MidpointRounding.AwayFromZero);
        }

        public async Task<int> GetRouteMiles(IList<LoadStopData> stopData)
        {
            if (stopData == null || stopData.Count() <= 1)
                throw new Exception("Invalid number of stops. Must provide 2 or more stops.");

            var formattedAddresses = stopData.Select(_ => FormatAddress(_)).ToList();
            var waypoints = formattedAddresses.Count() > 2 ? formattedAddresses.Skip(1).Take(formattedAddresses.Count - 2) : null;
            var waypointsQueryParam = waypoints != null ? $"&waypoints=optimize:true|{string.Join("|", waypoints)}" : "";

            var url = $"{_googleDirectionApiUrl}?" +
                $"origin={formattedAddresses.First()}" +
                $"&destination={formattedAddresses.Last()}" +
                $"{waypointsQueryParam}" +
                $"&key={_googleKey}";
            var apiResponse = await _httpClient.GetAsync(url);
            apiResponse.EnsureSuccessStatusCode();

            var content = await apiResponse.Content.ReadAsStringAsync();
            var directions = JsonConvert.DeserializeObject<GoogleDirectionClass>(content);
            var meters = directions?.routes?.FirstOrDefault()?.legs?.Select(_ => _.distance.value)?.Sum();

            return (int)Math.Round((meters ?? 0) / 1609.34, MidpointRounding.AwayFromZero);
        }

        private string FormatAddress(LoadStopData stopData)
        {
            return $"{stopData.Address1}, {stopData.City}, {stopData.State} {stopData.PostalCode}";
        }
    }
}
