using AutoMapper;
using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Validation.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using System.Net.Http;
using Newtonsoft.Json;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class SmartSpotPriceService : ISmartSpotPriceService
    {
        private const string ERROR_TYPE = "Smart Spot Price Service Error";
        private const string CONTENT_TYPE = "application/json";
        private const string CONTENT_TYPE_ENCODING = "application/json; charset=utf-8";

        private readonly SmartSpotPriceConfig _config;
        private readonly LoadshopDataContext _db;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly IUserContext _userContext;
        private readonly IRecaptchaService _recaptchaService;
        private readonly IMileageService _mileageService;
        private readonly ISecurityService _securityService;
        private readonly IShippingService _shippingService;
        private readonly ILoadCarrierGroupService _loadCarrierGroupService;

        public SmartSpotPriceService(SmartSpotPriceConfig config, LoadshopDataContext context, IMapper mapper,
            HttpClient httpClient,
            IUserContext userContext, 
            IRecaptchaService recaptchaService,
            IMileageService mileageService, 
            ISecurityService securityService,
            ILoadCarrierGroupService loadCarrierGroupService,
            IShippingService shippingService)
        {
            _config = config;
            _db = context;
            _mapper = mapper;
            _httpClient = httpClient;
            _userContext = userContext;
            _recaptchaService = recaptchaService;
            _mileageService = mileageService;
            _securityService = securityService;
            _loadCarrierGroupService = loadCarrierGroupService;
            _shippingService = shippingService;
        }

        public async Task<SmartSpotPrice> GetSmartSpotPriceAsync(LoadEntity load)
        {
            var carrierIds = new List<string>();
            // Get the load scacs along with the contracted scacs for that user and only send the intersection
            // this will mimic what the user sees on the POST screen
            var loadScacs = _shippingService.GetLoadCarrierScacs(load.LoadId);
            var contractedCarriers = await _securityService.GetContractedCarriersByPrimaryCustomerIdAsync();

            if (loadScacs == null || loadScacs.Count == 0)
            {
                // if no contracted carriers exist, use the load carrier group for this load, rank them and take the first one
                var carrierGroups = _loadCarrierGroupService.GetLoadCarrierGroupsForLoad(load.LoadId);

                var ranked = carrierGroups.Select(_ => new
                {
                    Group = _,
                    Rank = 0
                     + (_.OriginAddress1 != null ? 10000 : 0)
                     + (_.OriginCity != null ? 1000 : 0)
                     + (_.OriginState != null ? 100 : 0)
                     + (_.OriginCountry != null ? 10 : 0)
                     + (_.DestinationAddress1 != null ? 10000 : 0)
                     + (_.DestinationCity != null ? 1000 : 0)
                     + (_.DestinationState != null ? 100 : 0)
                     + (_.DestinationCountry != null ? 10 : 0)
                     + ((_.LoadCarrierGroupEquipment != null && _.LoadCarrierGroupEquipment?.Count(x => x.EquipmentId == load.EquipmentId) > 0) ? 1 : 0)
                }).ToList();

                carrierIds = ranked.OrderByDescending(x => x.Rank)
                                .FirstOrDefault()
                                ?.Group
                                .Carriers.Select(x => x.CarrierId).ToList() ?? new List<string>();

                if (carrierIds.Count == 0 && contractedCarriers != null)
                {
                    // select all contracted carriers
                    carrierIds = contractedCarriers.Select(x => x.CarrierId).ToList();
                }
            }
            else
            {
                var selectedScacs = loadScacs.Select(loadCarrierScac => loadCarrierScac.Scac);
                foreach (var carrier in contractedCarriers)
                {
                    var intersectingScacs = carrier.CarrierScacs.Where(scac => selectedScacs.Contains(scac));

                    if (intersectingScacs.Count() == carrier.CarrierScacs.Count())
                    {
                        carrierIds.Add(carrier.CarrierId);
                    }
                }
            }

            var smartSpotRequest = new List<LoadshopSmartSpotPriceRequest>()
            {
                new LoadshopSmartSpotPriceRequest()
                {
                    Commodity = load.Commodity,
                    EquipmentId = load.EquipmentId,
                    LoadId = load.LoadId,
                    Weight = load.Weight,
                    CarrierIds = carrierIds
                }
            };

            var results = await GetSmartSpotPricesAsync(smartSpotRequest);

            return results.FirstOrDefault();
        }

        public async Task<List<SmartSpotPrice>> GetSmartSpotPricesAsync(List<LoadshopSmartSpotPriceRequest> requests)
        {
            AssertConfig();
            if (requests == null) throw new ValidationException($"{ERROR_TYPE}: Requests are required");

            var awsModels = new List<AWSSmartSpotPriceRequest>();
            foreach (var request in requests)
            {
                var awsModel = await this.MapFromLoadshopSmartSpotPriceRequestAsync(request);
                awsModels.Add(awsModel);
            }

            return await GetSmartSpotPricesAsync(awsModels);
        }

        public async Task<List<SmartSpotPrice>> GetSmartSpotPricesAsync(List<AWSSmartSpotPriceRequest> requests)
        {
            AssertConfig();
            if (requests == null) throw new ValidationException($"{ERROR_TYPE}: Requests are required");

            var loadIds = new List<Guid>();
            foreach (var request in requests)
            {
                loadIds.Add(request.LoadId);
            }

            var awsPrices = await RequestPricesFromAWSAsync(loadIds, requests);
            var datGuardPrices = RequestDATGuardPricesAsync(loadIds, requests);

            for (var i = 0; i < awsPrices.Count; i++)
            {
                awsPrices[i].MachineLearningRate = awsPrices[i].Price;

                if (datGuardPrices.Count >= i + 1)
                {
                    awsPrices[i].DATGuardRate = datGuardPrices[i].Price;
                    // Return the lower of the DAT Guard Price and the AWS Smart Spot Price
                    if (datGuardPrices[i].Price > 0 && datGuardPrices[i].Price <= awsPrices[i].Price)
                    {
                        awsPrices[i].Price = datGuardPrices[i].Price;
                    }
                }
            }

            return awsPrices;
        }

        private List<SmartSpotPrice> RequestDATGuardPricesAsync(List<Guid> loadIds, List<AWSSmartSpotPriceRequest> awsModels)
        {
            var prices = new List<SmartSpotPrice>();
            for (var i = 0; i < loadIds.Count; i++)
            {
                var loadId = loadIds[i];
                var model = awsModels[i];

                var price = _db.GetDATGuardRate(model.OriginZip, model.DestZip, model.EquipmentId, model.PkupDate);

                prices.Add(new SmartSpotPrice { LoadId = loadId, Price = price });
            }
            return prices;
        }

        public async Task<GenericResponse<decimal?>> GetSmartSpotQuoteAsync(RecaptchaRequest<LoadshopSmartSpotQuoteRequest> request)
        {
            await _recaptchaService.ValidateToken(request);

            AssertConfig();

            var response = new GenericResponse<decimal?>();
            var awsModel = await MapFromLoadshopSmartSpotQuoteRequest(request.Data, response);

            if (!response.IsSuccess)
                return response;

            response.Data = await RequestQuoteFromAWS(awsModel);

            _db.Add(new SmartSpotPriceQuoteLogEntity
            {
                SmartSpotPriceQuoteLogId = Guid.NewGuid(),
                Miles = awsModel.DirectMiles,
                Weight = awsModel.Weight,
                EquipmentId = awsModel.EquipmentId,
                OrigState = awsModel.OrigState,
                Orig3Zip = awsModel.O3Zip,
                DestState = awsModel.DestState,
                Dest3Zip = awsModel.D3Zip,
                PkupDate = awsModel.PkupDate,
                SmartSpotPrice = response.Data ?? 0,
                UserId = _userContext.UserId
            });
            await _db.SaveChangesAsync(_userContext.UserName);

            return response;
        }


        private async Task<List<SmartSpotPrice>> RequestPricesFromAWSAsync(List<Guid> loadIds, List<AWSSmartSpotPriceRequest> awsModels)
        {
            AssertAWSModelsForPricing(loadIds, awsModels);

            return await RequestQuoteFromAWS(loadIds, awsModels);
        }

        private async Task<decimal> RequestQuoteFromAWS(AWSSmartSpotPriceRequest awsModel)
        {
            AssertAWSModelForQuote(awsModel);

            var quotes = await RequestQuoteFromAWS(new List<Guid> { Guid.Empty }, new List<AWSSmartSpotPriceRequest> { awsModel });
            var price = quotes.Select(_ => _.Price).Single();

            return price;
        }

        private async Task<List<SmartSpotPrice>> RequestQuoteFromAWS(List<Guid> loadIds, List<AWSSmartSpotPriceRequest> awsModels)
        {
            var awsUrl = new Uri(_config.ApiUrl);
            var host = awsUrl.Host;
            var region = _config.Region;
            var service = _config.Service;

            var method = "POST";
            var payload = "{\"data\": [\"" + string.Join(", ", awsModels.Select(x => x.ToString())) + "\" ]}";
            var payloadHash = AWS4SignerBase.CanonicalRequestHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var payloadHashString = AWS4SignerBase.ToHexString(payloadHash, true);
            var headers = new Dictionary<string, string>
            {
                {AWS4SignerBase.X_Amz_Content_SHA256, payloadHashString},
                {"Content-Length", payload.Length.ToString()},
                {"Content-Type", CONTENT_TYPE_ENCODING} // THIS MUST MATCH THE REQUEST MESSAGE.CONTENT.HEADERS "Content-Type" case and all
            };

            var signer = new AWS4SignerForAuthorizationHeader
            {
                EndpointUri = awsUrl,
                HttpMethod = method,
                Service = service,
                Region = region
            };
            var authorizationHeader = signer.ComputeSignature(headers,
                "",   // no query parameters
                payloadHashString,
                _config.AccessKeyId,
                _config.SecretAccessKey
            );

            var apiRequest = new HttpRequestMessage(HttpMethod.Post, awsUrl)
            {
                Content = new StringContent(payload, Encoding.UTF8, CONTENT_TYPE)
            };

            foreach (var header in headers.Keys)
            {
                apiRequest.Headers.TryAddWithoutValidation(header, headers[header]);
            }
            apiRequest.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

            var response = await _httpClient.SendAsync(apiRequest);
            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(msg))
                {
                    msg = "Unknown Error with Smart Spot Price Request";
                }
                throw new Exception($"{ERROR_TYPE}: {msg}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            if (responseContent.Contains("No SageMaker endpoints are currently up and running"))
            {
                throw new Exception($"{ERROR_TYPE}: {response.Content}");
            }

            var responseData = JsonConvert.DeserializeObject<AWSSmartSpotPriceResponse>(responseContent);

            if (responseData != null && responseData.Results != null && responseData.Results.Count >= awsModels.Count)
            {
                var prices = new List<SmartSpotPrice>();
                for (var i = 0; i < loadIds.Count; i++)
                {
                    var loadId = loadIds[i];
                    var price = responseData.Results[i];
                    prices.Add(new SmartSpotPrice { LoadId = loadId, Price = price });
                }
                return prices;
            }
            throw new Exception($"{ERROR_TYPE}: Unexpected Data returned from AWS: {response.Content}");
        }

        private async Task<AWSSmartSpotPriceRequest> MapFromLoadshopSmartSpotPriceRequestAsync(LoadshopSmartSpotPriceRequest request)
        {
            AssertRequest(request);
            var aws = new AWSSmartSpotPriceRequest();

            var load = await _db.Loads.AsNoTracking()
                .Include(x => x.LoadStops)
                .Include(x => x.CarrierScacs)
                .SingleOrDefaultAsync(x => x.LoadId == request.LoadId);

            if (load == null)
            {
                throw new Exception($"Unable to find load with ID {request.LoadId}.");
            }

            var origin = load.LoadStops.FirstOrDefault(x => x.StopNbr == 1);
            if (origin == null)
            {
                throw new Exception("Unable to find origin pickup stop");
            }
            var dest = load.LoadStops.FirstOrDefault(x => x.StopNbr == load.Stops);
            if (dest == null)
            {
                throw new Exception("Unable to find destination delivery stop");
            }

            aws.LoadId = request.LoadId;
            aws.TransactionCreate = DateTime.Now;
            aws.TransactionTypeId = load.LatestTransactionTypeId;
            aws.LoadShopMiles = load.Miles;
            aws.DirectMiles = load.DirectMiles;
            aws.Stops = load.LoadStops.Count;
            aws.Weight = request.Weight;
            aws.Commodity = request.Commodity;
            aws.EquipmentId = request.EquipmentId;
            aws.PkupDate = origin.LateDtTm;
            aws.OrigState = origin.State;
            aws.OriginZip = origin.PostalCode;
            aws.O3Zip = (origin.PostalCode != null && origin.PostalCode.Length >= 3) ? origin.PostalCode.Substring(0, 3) : null;
            aws.DestState = dest.State;
            aws.DestZip = dest.PostalCode;
            aws.D3Zip = (dest.PostalCode != null && dest.PostalCode.Length >= 3) ? dest.PostalCode.Substring(0, 3) : null;

            var carrierScacs = _db.CarrierScacs.Where(x => request.CarrierIds.Contains(x.CarrierId)).Select(x => x.Scac).Distinct().ToList();
            aws.NbrSCACsRequest = carrierScacs.Count;
            aws.NbrCarriersRequest = request.CarrierIds.Count;

            aws.NbrSCACsPosted = load.CarrierScacs
                .Where(x => x.ContractRate == null || x.ContractRate.Value <= load.LineHaulRate)
                .Select(x => x.Scac)
                .Distinct()
                .Count();

            aws.NbrContractSCACsPosted = load.CarrierScacs
                .Where(x => x.ContractRate != null && x.ContractRate.Value <= load.LineHaulRate)
                .Select(x => x.Scac)
                .Distinct()
                .Count();

            aws.NbrSCACsHidden = load.CarrierScacs
                .Where(x => x.ContractRate != null && x.ContractRate.Value > load.LineHaulRate)
                .Select(x => x.Scac)
                .Distinct()
                .Count();

            return aws;
        }

        private async Task<AWSSmartSpotPriceRequest> MapFromLoadshopSmartSpotQuoteRequest(LoadshopSmartSpotQuoteRequest request, BaseServiceResponse response)
        {
            AssertRequest(request, response);
            if (!response.IsSuccess)
                return null;

            var (routeMiles, directMiles) = await GetMileage(request);
            var carriers = (await _securityService.GetContractedCarriersByPrimaryCustomerIdAsync());
            var carrierCount = carriers?.Count() ?? 1;
            var distinctScacCount = carriers?.SelectMany(_ => _.CarrierScacs).Distinct().Count() ?? 1;


            var aws = new AWSSmartSpotPriceRequest();

            aws.LoadId = Guid.Empty;
            aws.TransactionCreate = DateTime.Now;
            aws.TransactionTypeId = "New";
            aws.LoadShopMiles = routeMiles;
            aws.DirectMiles = directMiles;
            aws.Stops = 2;
            aws.Weight = request.Weight.Value;
            aws.Commodity = "Misc/Other";
            aws.EquipmentId = request.EquipmentId;
            aws.PkupDate = request.PickupDate.Value.Date;
            aws.OrigState = request.OriginState;
            aws.O3Zip = request.OriginPostalCode.Length >= 3 ? request.OriginPostalCode.Substring(0, 3) : null;
            aws.DestState = request.DestinationState;
            aws.D3Zip = request.DestinationPostalCode.Length >= 3 ? request.DestinationPostalCode.Substring(0, 3) : null;

            aws.NbrSCACsRequest = distinctScacCount;
            aws.NbrCarriersRequest = carrierCount;
            aws.NbrSCACsPosted = 0;
            aws.NbrContractSCACsPosted = 0;
            aws.NbrSCACsHidden = 0;

            return aws;
        }

        private void AssertConfig()
        {
            if (string.IsNullOrWhiteSpace(_config.ApiUrl)) throw new Exception($"{ERROR_TYPE}: Missing API URL Config Setting");
            if (string.IsNullOrWhiteSpace(_config.AccessKeyId)) throw new Exception($"{ERROR_TYPE}: Missing Access Key Id Config Setting");
            if (string.IsNullOrWhiteSpace(_config.SecretAccessKey)) throw new Exception($"{ERROR_TYPE}: Missing Secret Access Key Config Setting");
            if (string.IsNullOrWhiteSpace(_config.Service)) throw new Exception($"{ERROR_TYPE}: Missing Service Config Setting");
            if (string.IsNullOrWhiteSpace(_config.Region)) throw new Exception($"{ERROR_TYPE}: Missing Region Config Setting");
        }

        private void AssertRequest(LoadshopSmartSpotPriceRequest request)
        {
            if (request == null) throw new ValidationException($"{ERROR_TYPE}: Request is null");
            if (request.LoadId == Guid.Empty) throw new ValidationException($"{ERROR_TYPE}: {nameof(request.LoadId)} is required");
            if (string.IsNullOrWhiteSpace(request.Commodity)) throw new ValidationException($"{ERROR_TYPE}: {nameof(request.Commodity)} is required");
            if (string.IsNullOrWhiteSpace(request.EquipmentId)) throw new ValidationException($"{ERROR_TYPE}: {nameof(request.EquipmentId)} is required");
        }

        private void AssertRequest(LoadshopSmartSpotQuoteRequest request, BaseServiceResponse response)
        {
            if (request == null) throw new ValidationException($"{ERROR_TYPE}: Request is required");

            if (string.IsNullOrWhiteSpace(request.EquipmentId))
                response.AddError($"urn:root:{nameof(request.EquipmentId)}", "Equipment is required");

            if (request.Weight == null || request.Weight <= 0)
                response.AddError($"urn:root:{nameof(request.Weight)}", "Weight is required");

            if (request.PickupDate == null || request.PickupDate.Equals(DateTimeOffset.MinValue))
                response.AddError($"urn:root:{nameof(request.PickupDate)}", "Date is required");

            if (string.IsNullOrWhiteSpace(request.OriginPostalCode))
                response.AddError($"urn:root:{nameof(request.OriginPostalCode)}", "Origin Postal Code is required");
            else if (string.IsNullOrWhiteSpace(request.OriginState))
                response.AddError($"urn:root:{nameof(request.OriginState)}", "Origin State is required");
            else if (string.IsNullOrWhiteSpace(request.OriginCountry))
                response.AddError($"urn:root:{nameof(request.OriginCountry)}", "Origin Country is required");

            if (string.IsNullOrWhiteSpace(request.DestinationPostalCode))
                response.AddError($"urn:root:{nameof(request.DestinationPostalCode)}", "Destination Postal Code is required");
            else if (string.IsNullOrWhiteSpace(request.DestinationState))
                response.AddError($"urn:root:{nameof(request.DestinationState)}", "Destination State is required");
            else if (string.IsNullOrWhiteSpace(request.DestinationCountry))
                response.AddError($"urn:root:{nameof(request.DestinationCountry)}", "Destination Country is required");

        }

        private void AssertAWSModelsForPricing(List<Guid> loadIds, List<AWSSmartSpotPriceRequest> awsModels)
        {
            if (loadIds == null || !loadIds.Any()) throw new ValidationException($"{ERROR_TYPE}: Load IDs are required");
            if (awsModels == null || !awsModels.Any()) throw new ValidationException($"{ERROR_TYPE}: AWS Models are required");
            if (awsModels.Count != loadIds.Count) throw new ValidationException($"{ERROR_TYPE}: Number of LoadIds and AWS Models must be the same");
            foreach (var awsModel in awsModels)
            {
                if (awsModel.LoadId == Guid.Empty) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.LoadId)} is required");
                if (awsModel.TransactionCreate.Equals(DateTime.MinValue)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.TransactionCreate)} is required");
                if (string.IsNullOrWhiteSpace(awsModel.TransactionTypeId)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.TransactionTypeId)} is required");
                if (string.IsNullOrWhiteSpace(awsModel.Commodity)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.Commodity)} is required");
                if (string.IsNullOrWhiteSpace(awsModel.EquipmentId)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.EquipmentId)} is required");
                if (awsModel.PkupDate.Equals(DateTime.MinValue)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.PkupDate)} is required");

                if (string.IsNullOrWhiteSpace(awsModel.OrigState)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.OrigState)} is required");
                if (string.IsNullOrWhiteSpace(awsModel.O3Zip)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.O3Zip)} is required");
                if (string.IsNullOrWhiteSpace(awsModel.DestState)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.DestState)} is required");
                if (string.IsNullOrWhiteSpace(awsModel.D3Zip)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.D3Zip)} is required");
            }
        }

        private void AssertAWSModelForQuote(AWSSmartSpotPriceRequest awsModel)
        {
            if (awsModel == null) throw new ValidationException($"{ERROR_TYPE}: AWS Model is required");
            if (awsModel.LoadId != Guid.Empty) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.LoadId)} is must be empty");
            if (awsModel.TransactionCreate.Equals(DateTime.MinValue)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.TransactionCreate)} is required");
            if (string.IsNullOrWhiteSpace(awsModel.TransactionTypeId)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.TransactionTypeId)} is required");
            if (string.IsNullOrWhiteSpace(awsModel.Commodity)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.Commodity)} is required");
            if (string.IsNullOrWhiteSpace(awsModel.EquipmentId)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.EquipmentId)} is required");
            if (awsModel.PkupDate.Equals(DateTime.MinValue)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.PkupDate)} is required");

            if (string.IsNullOrWhiteSpace(awsModel.OrigState)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.OrigState)} is required");
            if (string.IsNullOrWhiteSpace(awsModel.O3Zip)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.O3Zip)} is required");
            if (string.IsNullOrWhiteSpace(awsModel.DestState)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.DestState)} is required");
            if (string.IsNullOrWhiteSpace(awsModel.D3Zip)) throw new ValidationException($"{ERROR_TYPE}: {nameof(awsModel.D3Zip)} is required");
        }

        private async Task<(int, int)> GetMileage(LoadshopSmartSpotQuoteRequest request)
        {
            var mileageRequest = new MileageRequestData
            {
                OriginCity = request.OriginCity,
                OriginState = request.OriginState,
                OriginPostalCode = request.OriginPostalCode,
                OriginCountry = request.OriginCountry,

                DestinationCity = request.DestinationCity,
                DestinationState = request.DestinationState,
                DestinationPostalCode = request.DestinationPostalCode,
                DestinationCountry = request.DestinationCountry
            };

            var routeMiles = await _mileageService.GetDirectRouteMiles(mileageRequest);
            var directMiles = _mileageService.GetDirectMiles(mileageRequest);
            if (directMiles <= 0)
            {
                directMiles = routeMiles;
            }
            return (routeMiles, directMiles);
        }
    }

    /// <summary>
    /// Below code taken from https://docs.aws.amazon.com/AmazonS3/latest/API/sig-v4-examples-using-sdks.html#sig-v4-examples-using-sdk-dotnet
    /// </summary>
    public class AWS4SignerForAuthorizationHeader : AWS4SignerBase
    {
        /// <summary>
        /// Computes an AWS4 signature for a request, ready for inclusion as an 
        /// 'Authorization' header.
        /// </summary>
        /// <param name="headers">
        /// The request headers; 'Host' and 'X-Amz-Date' will be added to this set.
        /// </param>
        /// <param name="queryParameters">
        /// Any query parameters that will be added to the endpoint. The parameters 
        /// should be specified in canonical format.
        /// </param>
        /// <param name="bodyHash">
        /// Precomputed SHA256 hash of the request body content; this value should also
        /// be set as the header 'X-Amz-Content-SHA256' for non-streaming uploads.
        /// </param>
        /// <param name="awsAccessKey">
        /// The user's AWS Access Key.
        /// </param>
        /// <param name="awsSecretKey">
        /// The user's AWS Secret Key.
        /// </param>
        /// <returns>
        /// The computed authorization string for the request. This value needs to be set as the 
        /// header 'Authorization' on the subsequent HTTP request.
        /// </returns>
        public string ComputeSignature(IDictionary<string, string> headers,
                                       string queryParameters,
                                       string bodyHash,
                                       string awsAccessKey,
                                       string awsSecretKey)
        {
            // first get the date and time for the subsequent request, and convert to ISO 8601 format
            // for use in signature generation
            var requestDateTime = DateTime.UtcNow;
            var dateTimeStamp = requestDateTime.ToString(ISO8601BasicFormat, CultureInfo.InvariantCulture);

            // update the headers with required 'x-amz-date' and 'host' values
            headers.Add(X_Amz_Date, dateTimeStamp);

            var hostHeader = EndpointUri.Host;
            if (!EndpointUri.IsDefaultPort)
                hostHeader += ":" + EndpointUri.Port;
            headers.Add("Host", hostHeader);

            // canonicalize the headers; we need the set of header names as well as the
            // names and values to go into the signature process
            var canonicalizedHeaderNames = CanonicalizeHeaderNames(headers);
            var canonicalizedHeaders = CanonicalizeHeaders(headers);

            // if any query string parameters have been supplied, canonicalize them
            // (note this sample assumes any required url encoding has been done already)
            var canonicalizedQueryParameters = string.Empty;
            if (!string.IsNullOrEmpty(queryParameters))
            {
                var paramDictionary = queryParameters.Split('&').Select(p => p.Split('='))
                                                     .ToDictionary(nameval => nameval[0],
                                                                   nameval => nameval.Length > 1
                                                                        ? nameval[1] : "");

                var sb = new StringBuilder();
                var paramKeys = new List<string>(paramDictionary.Keys);
                paramKeys.Sort(StringComparer.Ordinal);
                foreach (var p in paramKeys)
                {
                    if (sb.Length > 0)
                        sb.Append("&");
                    sb.AppendFormat("{0}={1}", p, paramDictionary[p]);
                }

                canonicalizedQueryParameters = sb.ToString();
            }

            // canonicalize the various components of the request
            var canonicalRequest = CanonicalizeRequest(EndpointUri,
                                                       HttpMethod,
                                                       canonicalizedQueryParameters,
                                                       canonicalizedHeaderNames,
                                                       canonicalizedHeaders,
                                                       bodyHash);
            Console.WriteLine("\nCanonicalRequest:\n{0}", canonicalRequest);

            // generate a hash of the canonical request, to go into signature computation
            var canonicalRequestHashBytes
                = CanonicalRequestHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(canonicalRequest));

            // construct the string to be signed
            var stringToSign = new StringBuilder();

            var dateStamp = requestDateTime.ToString(DateStringFormat, CultureInfo.InvariantCulture);
            var scope = string.Format("{0}/{1}/{2}/{3}",
                                      dateStamp,
                                      Region,
                                      Service,
                                      TERMINATOR);

            stringToSign.AppendFormat("{0}-{1}\n{2}\n{3}\n", SCHEME, ALGORITHM, dateTimeStamp, scope);
            stringToSign.Append(ToHexString(canonicalRequestHashBytes, true));

            Console.WriteLine("\nStringToSign:\n{0}", stringToSign);

            // compute the signing key
            var kha = KeyedHashAlgorithm.Create(HMACSHA256);
            kha.Key = DeriveSigningKey(HMACSHA256, awsSecretKey, Region, dateStamp, Service);

            // compute the AWS4 signature and return it
            var signature = kha.ComputeHash(Encoding.UTF8.GetBytes(stringToSign.ToString()));
            var signatureString = ToHexString(signature, true);
            Console.WriteLine("\nSignature:\n{0}", signatureString);

            var authString = new StringBuilder();
            authString.AppendFormat("{0}-{1} ", SCHEME, ALGORITHM);
            authString.AppendFormat("Credential={0}/{1}, ", awsAccessKey, scope);
            authString.AppendFormat("SignedHeaders={0}, ", canonicalizedHeaderNames);
            authString.AppendFormat("Signature={0}", signatureString);

            var authorization = authString.ToString();
            Console.WriteLine("\nAuthorization:\n{0}", authorization);

            return authorization;
        }
    }

    /// <summary>
    /// Common methods and properties for all AWS4 signer variants
    /// </summary>
    public abstract class AWS4SignerBase
    {
        // SHA256 hash of an empty request body
        public const string EMPTY_BODY_SHA256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

        public const string SCHEME = "AWS4";
        public const string ALGORITHM = "HMAC-SHA256";
        public const string TERMINATOR = "aws4_request";

        // format strings for the date/time and date stamps required during signing
        public const string ISO8601BasicFormat = "yyyyMMddTHHmmssZ";
        public const string DateStringFormat = "yyyyMMdd";

        // some common x-amz-* parameters
        public const string X_Amz_Algorithm = "X-Amz-Algorithm";
        public const string X_Amz_Credential = "X-Amz-Credential";
        public const string X_Amz_SignedHeaders = "X-Amz-SignedHeaders";
        public const string X_Amz_Date = "X-Amz-Date";
        public const string X_Amz_Signature = "X-Amz-Signature";
        public const string X_Amz_Expires = "X-Amz-Expires";
        public const string X_Amz_Content_SHA256 = "X-Amz-Content-SHA256";
        public const string X_Amz_Decoded_Content_Length = "X-Amz-Decoded-Content-Length";
        public const string X_Amz_Meta_UUID = "X-Amz-Meta-UUID";

        // the name of the keyed hash algorithm used in signing
        public const string HMACSHA256 = "HMACSHA256";

        // request canonicalization requires multiple whitespace compression
        protected static readonly Regex CompressWhitespaceRegex = new Regex("\\s+");

        // algorithm used to hash the canonical request that is supplied to
        // the signature computation
        public static HashAlgorithm CanonicalRequestHashAlgorithm = HashAlgorithm.Create("SHA-256");

        /// <summary>
        /// The service endpoint, including the path to any resource.
        /// </summary>
        public Uri EndpointUri { get; set; }

        /// <summary>
        /// The HTTP verb for the request, e.g. GET.
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// The signing name of the service, e.g. 's3'.
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// The system name of the AWS region associated with the endpoint, e.g. us-east-1.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Returns the canonical collection of header names that will be included in
        /// the signature. For AWS4, all header names must be included in the process 
        /// in sorted canonicalized order.
        /// </summary>
        /// <param name="headers">
        /// The set of header names and values that will be sent with the request
        /// </param>
        /// <returns>
        /// The set of header names canonicalized to a flattened, ;-delimited string
        /// </returns>
        protected string CanonicalizeHeaderNames(IDictionary<string, string> headers)
        {
            var headersToSign = new List<string>(headers.Keys);
            headersToSign.Sort(StringComparer.OrdinalIgnoreCase);

            var sb = new StringBuilder();
            foreach (var header in headersToSign)
            {
                if (sb.Length > 0)
                    sb.Append(";");
                sb.Append(header.ToLower());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Computes the canonical headers with values for the request. 
        /// For AWS4, all headers must be included in the signing process.
        /// </summary>
        /// <param name="headers">The set of headers to be encoded</param>
        /// <returns>Canonicalized string of headers with values</returns>
        protected virtual string CanonicalizeHeaders(IDictionary<string, string> headers)
        {
            if (headers == null || headers.Count == 0)
                return string.Empty;

            // step1: sort the headers into lower-case format; we create a new
            // map to ensure we can do a subsequent key lookup using a lower-case
            // key regardless of how 'headers' was created.
            var sortedHeaderMap = new SortedDictionary<string, string>();
            foreach (var header in headers.Keys)
            {
                sortedHeaderMap.Add(header.ToLower(), headers[header]);
            }

            // step2: form the canonical header:value entries in sorted order. 
            // Multiple white spaces in the values should be compressed to a single 
            // space.
            var sb = new StringBuilder();
            foreach (var header in sortedHeaderMap.Keys)
            {
                var headerValue = CompressWhitespaceRegex.Replace(sortedHeaderMap[header], " ");
                sb.AppendFormat("{0}:{1}\n", header, headerValue.Trim());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns the canonical request string to go into the signer process; this 
        /// consists of several canonical sub-parts.
        /// </summary>
        /// <param name="endpointUri"></param>
        /// <param name="httpMethod"></param>
        /// <param name="queryParameters"></param>
        /// <param name="canonicalizedHeaderNames">
        /// The set of header names to be included in the signature, formatted as a flattened, ;-delimited string
        /// </param>
        /// <param name="canonicalizedHeaders">
        /// </param>
        /// <param name="bodyHash">
        /// Precomputed SHA256 hash of the request body content. For chunked encoding this
        /// should be the fixed string ''.
        /// </param>
        /// <returns>String representing the canonicalized request for signing</returns>
        protected string CanonicalizeRequest(Uri endpointUri,
                                             string httpMethod,
                                             string queryParameters,
                                             string canonicalizedHeaderNames,
                                             string canonicalizedHeaders,
                                             string bodyHash)
        {
            var canonicalRequest = new StringBuilder();

            canonicalRequest.AppendFormat("{0}\n", httpMethod);
            canonicalRequest.AppendFormat("{0}\n", CanonicalResourcePath(endpointUri));
            canonicalRequest.AppendFormat("{0}\n", queryParameters);

            canonicalRequest.AppendFormat("{0}\n", canonicalizedHeaders);
            canonicalRequest.AppendFormat("{0}\n", canonicalizedHeaderNames);

            canonicalRequest.Append(bodyHash);

            return canonicalRequest.ToString();
        }

        /// <summary>
        /// Returns the canonicalized resource path for the service endpoint
        /// </summary>
        /// <param name="endpointUri">Endpoint to the service/resource</param>
        /// <returns>Canonicalized resource path for the endpoint</returns>
        protected string CanonicalResourcePath(Uri endpointUri)
        {
            if (string.IsNullOrEmpty(endpointUri.AbsolutePath))
                return "/";

            // encode the path per RFC3986
            return endpointUri.AbsolutePath;
        }

        /// <summary>
        /// Compute and return the multi-stage signing key for the request.
        /// </summary>
        /// <param name="algorithm">Hashing algorithm to use</param>
        /// <param name="awsSecretAccessKey">The clear-text AWS secret key</param>
        /// <param name="region">The region in which the service request will be processed</param>
        /// <param name="date">Date of the request, in yyyyMMdd format</param>
        /// <param name="service">The name of the service being called by the request</param>
        /// <returns>Computed signing key</returns>
        protected byte[] DeriveSigningKey(string algorithm, string awsSecretAccessKey, string region, string date, string service)
        {
            const string ksecretPrefix = SCHEME;
            char[] ksecret = null;

            ksecret = (ksecretPrefix + awsSecretAccessKey).ToCharArray();

            byte[] hashDate = ComputeKeyedHash(algorithm, Encoding.UTF8.GetBytes(ksecret), Encoding.UTF8.GetBytes(date));
            byte[] hashRegion = ComputeKeyedHash(algorithm, hashDate, Encoding.UTF8.GetBytes(region));
            byte[] hashService = ComputeKeyedHash(algorithm, hashRegion, Encoding.UTF8.GetBytes(service));
            return ComputeKeyedHash(algorithm, hashService, Encoding.UTF8.GetBytes(TERMINATOR));
        }

        /// <summary>
        /// Compute and return the hash of a data blob using the specified algorithm
        /// and key
        /// </summary>
        /// <param name="algorithm">Algorithm to use for hashing</param>
        /// <param name="key">Hash key</param>
        /// <param name="data">Data blob</param>
        /// <returns>Hash of the data</returns>
        protected byte[] ComputeKeyedHash(string algorithm, byte[] key, byte[] data)
        {
            var kha = KeyedHashAlgorithm.Create(algorithm);
            kha.Key = key;
            return kha.ComputeHash(data);
        }

        /// <summary>
        /// Helper to format a byte array into string
        /// </summary>
        /// <param name="data">The data blob to process</param>
        /// <param name="lowercase">If true, returns hex digits in lower case form</param>
        /// <returns>String version of the data</returns>
        public static string ToHexString(byte[] data, bool lowercase)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString(lowercase ? "x2" : "X2"));
            }
            return sb.ToString();
        }
    }
}
