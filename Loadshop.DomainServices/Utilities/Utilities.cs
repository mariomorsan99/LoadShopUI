using System;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Common.Services;
using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Utilities.Models.Visibility;
using Loadshop.Data;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Security;
using Microsoft.EntityFrameworkCore;
using Loadshop.API.Models;

namespace Loadshop.DomainServices.Utilities
{
    public class Utilities
    {
        private readonly LoadshopDataContext _context;
        private readonly IConfigurationRoot _config;
        private readonly HttpClient _client;
        private readonly IUserContext _userContext;
        private readonly ICommonService _commonService;
        private readonly ILoadService _loadService;

        public Utilities(LoadshopDataContext context, IConfigurationRoot config, HttpClient client, IUserContext userContext, ICommonService commonService, ILoadService loadService)
        {
            _config = config;
            _client = client;
            _context = context;
            _userContext = userContext;
            _commonService = commonService;
            _loadService = loadService;
        }

        public List<string> GetVisibilityTypes(Guid userId)
        {
            var user = _context.Users
                .Include(u => u.PrimaryScacEntity)
                .SingleOrDefault(x => x.IdentUserId == userId);

            if (user == null)
            {
                throw new Exception("Invalid User");
            }

            if (user.PrimaryScacEntity == null)
                return null;

            return _commonService.GetCarrierVisibilityTypes(user.Username, user.PrimaryScacEntity?.CarrierId);
        }

        public void CheckLoadsForVisibility(List<LoadViewData> loads, string token, List<string> visibilityTypes)
        {
            string topsToGoErrors = null;
            string project44Errors = null;

            foreach (var load in loads)
            {
                var lte = _loadService.GetLatestTransaction(load.LoadId);

                if (lte.TransactionTypeId != "Accepted")
                {
                    continue;
                }

                if (visibilityTypes.Contains(CarrierVisibilityTypes.TopsToGo))
                    CheckLoadsForTopsToGoVisibilityData(load, lte, token, ref topsToGoErrors);

                if (visibilityTypes.Contains(CarrierVisibilityTypes.Project44))
                    CheckLoadsForProject44VisibilityData(load, lte, token, ref project44Errors);
            }

            //Save all successful updates
            _context.SaveChanges("system");

            //Throw any accumulated errors
            if (!string.IsNullOrWhiteSpace(topsToGoErrors) || !string.IsNullOrWhiteSpace(project44Errors))
            {
                StringBuilder errors = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(topsToGoErrors))
                {
                    errors.Append(topsToGoErrors);
                }

                if (!string.IsNullOrWhiteSpace(project44Errors))
                {
                    if (errors.Length > 0)
                    {
                        errors.AppendLine(project44Errors);
                    }
                    else
                    {
                        errors.Append(project44Errors);
                    }
                }

                throw new Exception(errors.ToString());
            }
        }

        private void CheckLoadsForTopsToGoVisibilityData(LoadViewData load, LoadTransactionEntity lte, string token, ref string topsToGoErrors)
        {
            if (string.IsNullOrWhiteSpace(load.BillingLoadDisplay))
                return;

            const string mediaType = "application/json";

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
            _client.DefaultRequestHeaders.Add("Authorization", token);

            var url = string.Format(_config.GetValue<string>("MobileApiAddress") + "t2g/api/sms/loadinfo/{0}", load.BillingLoadDisplay);

            var responseMessage = _client.GetAsync(url, default(CancellationToken)).Result;
            var responseString = responseMessage.Content.ReadAsStringAsync().Result.Replace("\"e\":", "Errors:").Replace("\"d\":", "Data:").Replace("\"s\":", "Success:");
            var response = JsonConvert.DeserializeObject<ResponseMessage<List<TopsToGoDetail>>>(responseString);

            if (!responseMessage.IsSuccessStatusCode || response == null || response.Success == false || response.Errors.Count > 0)
            {
                topsToGoErrors = $"TopsToGo data retrieval failed using endpoint, {url}, for load: {load.ReferenceLoadDisplay}";

                if (response != null && response.Errors.Count > 0)
                {
                    var errors = response.Errors.Aggregate(new StringBuilder(), (s, r) => s.AppendLine(r.Message)).ToString();
                    topsToGoErrors += " - " + errors;
                }

                return;
            }

            if (response.Data.Count > 0)
            {
                var topsToGoDetailInfo = response.Data[0];

                if (topsToGoDetailInfo.AccessExpiredUtc > DateTime.UtcNow)
                {
                    if (string.IsNullOrWhiteSpace(lte.Claim.VisibilityPhoneNumber) ||
                        (lte.Claim.VisibilityChgDtTm.HasValue && lte.Claim.VisibilityChgDtTm.Value < topsToGoDetailInfo.SendUtc.ToLocalTime()))
                    {
                        load.ShowVisibilityWarning = false;
                        load.MobileExternallyEntered = true;
                        load.VisibilityChgDtTm = topsToGoDetailInfo.SendUtc.ToLocalTime();
                        lte.Claim.MobileExternallyEntered = load.MobileExternallyEntered;
                        lte.Claim.VisibilityChgDtTm = load.VisibilityChgDtTm;
                    }
                }
            }
        }

        private void CheckLoadsForProject44VisibilityData(LoadViewData load, LoadTransactionEntity lte, string token, ref string project44Errors)
        {
            if (string.IsNullOrWhiteSpace(load.BillingLoadDisplay))
                return;

            const string mediaType = "application/json";
            string url = _config.GetValue<string>("VisibilityApiAddress") + "asset/find-asset";

            var formData = JsonConvert.SerializeObject(new Project44AssetInfo { LoadId = load.BillingLoadId });
            var content = new StringContent(formData, Encoding.UTF8, mediaType);

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", token);

            var responseMessage = _client.PostAsync(url, content, default(CancellationToken)).Result;
            var responseString = responseMessage.Content.ReadAsStringAsync().Result;

            if (!responseMessage.IsSuccessStatusCode)
            {
                project44Errors = $"Project44 data retrieval failed using endpoint, {url}, for load: {load.ReferenceLoadDisplay}. Reason: {responseMessage.ReasonPhrase}";

                return;
            }

            var project44AssetInfo = JsonConvert.DeserializeObject<Project44AssetInfo>(responseString);

            if (!string.IsNullOrWhiteSpace(project44AssetInfo.AssetId))
            {
                if (string.IsNullOrWhiteSpace(lte.Claim.VisibilityTruckNumber) ||
                    (lte.Claim.VisibilityTruckNumber != project44AssetInfo.AssetId && lte.Claim.VisibilityChgDtTm < project44AssetInfo.AssetLastChgDtTm))
                {
                    load.ShowVisibilityWarning = false;
                    load.VisibilityTruckNumber = project44AssetInfo.AssetId;
                    load.VisibilityChgDtTm = project44AssetInfo.AssetLastChgDtTm;
                    lte.Claim.VisibilityTruckNumber = load.VisibilityTruckNumber;
                    lte.Claim.VisibilityChgDtTm = load.VisibilityChgDtTm;
                }
            }
        }

        public LoadClaimEntity UpdateVisibilityData(LoadData load, Guid userId)
        {
            var user = _context.Users
                .Include(u => u.PrimaryScacEntity)
                .SingleOrDefault(x => x.IdentUserId == userId);

            var latestLoadClaim = GetLatestLoadClaim(load.LoadId.Value);

            if (user == null)
            {
                throw new Exception("Invalid User");
            }

            if (latestLoadClaim == null)
            {
                throw new ValidationException("Could not find Load Claim associated with load.");
            }

            if (user.PrimaryScacEntity?.CarrierId != latestLoadClaim.CarrierScac.CarrierId)
            {
                throw new ValidationException($"{user.Username} is not allowed to update this load");
            }

            var visibilityTypes = _commonService.GetCarrierVisibilityTypes(user.Username, user.PrimaryScacEntity?.CarrierId);

            if (visibilityTypes == null || visibilityTypes.Count == 0)
            {
                throw new ValidationException("No visibility access");
            }

            var t = _loadService.GetLatestTransaction(load.LoadId.Value);

            if (t.TransactionTypeId != "Accepted")
            {
                throw new ValidationException("Not a valid Accepted load");
            }

            string topsToGoErrors = null;
            string project44Errors = null;
            var updateVisibilityPhoneNumber = !string.IsNullOrWhiteSpace(load.VisibilityPhoneNumber) && t.Claim.VisibilityPhoneNumber != load.VisibilityPhoneNumber;
            var updateVisibilityTruckNumber = !string.IsNullOrWhiteSpace(load.VisibilityTruckNumber) && t.Claim.VisibilityTruckNumber != load.VisibilityTruckNumber;

            if (updateVisibilityPhoneNumber)
            {
                if (string.IsNullOrWhiteSpace(load.BillingLoadDisplay))
                {
                    throw new ValidationException("Invalid BillingLoadID");
                }

                topsToGoErrors = "";
                UpdateTopsToGo(load.VisibilityPhoneNumber, load.BillingLoadDisplay, _userContext.Token, "validation", ref topsToGoErrors);

                if (string.IsNullOrWhiteSpace(topsToGoErrors))
                {
                    UpdateTopsToGo(load.VisibilityPhoneNumber, load.BillingLoadDisplay, _userContext.Token, "save", ref topsToGoErrors);

                    if (string.IsNullOrWhiteSpace(topsToGoErrors))
                    {
                        t.Claim.VisibilityPhoneNumber = load.VisibilityPhoneNumber;
                        t.Claim.MobileExternallyEntered = false;
                        t.Claim.VisibilityChgDtTm = DateTime.Now;
                    }
                }
                else
                {
                    load.VisibilityPhoneNumber = t.Claim.VisibilityPhoneNumber;
                }
            }

            if (updateVisibilityTruckNumber)
            {
                if (string.IsNullOrWhiteSpace(load.BillingLoadId))
                {
                    throw new ValidationException("Invalid BillingLoadID");
                }

                project44Errors = "";
                UpdateProject44(load.VisibilityTruckNumber, load.BillingLoadId, _userContext.Token, ref project44Errors);

                if (string.IsNullOrWhiteSpace(project44Errors))
                {
                    t.Claim.VisibilityTruckNumber = load.VisibilityTruckNumber;
                    t.Claim.VisibilityChgDtTm = DateTime.Now;
                }
            }

            if ((updateVisibilityPhoneNumber && string.IsNullOrWhiteSpace(topsToGoErrors)) || (updateVisibilityTruckNumber && string.IsNullOrWhiteSpace(project44Errors)))
            {
                _context.SaveChanges(user.Username);
            }

            if (!string.IsNullOrWhiteSpace(topsToGoErrors) || !string.IsNullOrWhiteSpace(project44Errors))
            {
                StringBuilder errors = new StringBuilder();
                errors.Append(!string.IsNullOrWhiteSpace(topsToGoErrors) ? topsToGoErrors : "");
                errors.Append(errors.Length > 0 && !string.IsNullOrWhiteSpace(project44Errors) ? "<br/><br/>*****<br/><br/>" : "");
                errors.Append(!string.IsNullOrWhiteSpace(project44Errors) ? project44Errors : "");

                throw new ValidationException(errors.ToString());
            }

            return (t.Claim);
        }

        private void UpdateTopsToGo(string phoneNumber, string loadNbr, string token, string processingType, ref string topsToGoErrors)
        {
            const string mediaType = "application/json";
            string url;
            var cleansedPhoneNumber = phoneNumber.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", "");

            if (processingType == "validation")
            {
                url = string.Format(_config.GetValue<string>("MobileApiAddress") + "t2g/api/sms/validatephone?phoneNumber={0}", cleansedPhoneNumber ?? "");
            }
            else if (processingType == "save")
            {
                url = string.Format(_config.GetValue<string>("MobileApiAddress") + "t2g/api/sms/send?phoneNumber={0}&loadNbr={1}", cleansedPhoneNumber, loadNbr);
            }
            else
            {
                throw new ValidationException("Invalid TopsToGo processing type - " + processingType + ". Expecting \"validation\" or \"save\".");
            }

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
            _client.DefaultRequestHeaders.Add("Authorization", token);

            var responseMessage = _client.PostAsync(url, new StringContent(""), default(CancellationToken)).Result;
            var responseString = responseMessage.Content.ReadAsStringAsync().Result;

            if (processingType == "validation")
            {
                var response = JsonConvert.DeserializeObject<ResponseMessage<AuthlessTextMaintModel.Data>>(responseString);

                if (!responseMessage.IsSuccessStatusCode || response == null || response.Success == false || response.Errors.Count > 0 || (response.Data != null && response.Data.Item2.PhoneType.ToUpper() != "MOBILE"))
                {
                    topsToGoErrors = $"Phone number {processingType} failed";

                    if (response != null)
                    {
                        if (response.Data != null && response.Data.Item2.PhoneType.ToUpper() != "MOBILE")
                        {
                            topsToGoErrors += " - please specify a mobile number; " + phoneNumber + " is a " + response.Data.Item2.PhoneType.ToLower() + " number";
                        }

                        if (response.Errors.Count > 0)
                        {
                            var errors = response.Errors.Aggregate(new StringBuilder(), (s, r) => s.AppendLine(r.Message)).ToString();
                            topsToGoErrors += " - " + errors;
                        }
                    }
                }
            }
            else
            {
                var response = JsonConvert.DeserializeObject<ResponseMessage<bool>>(responseString);

                if (!responseMessage.IsSuccessStatusCode || response == null || response.Success == false || response.Errors.Count > 0)
                {
                    topsToGoErrors = $"Phone number {processingType} failed";

                    if (response != null && response.Errors.Count > 0)
                    {
                        var errors = response.Errors.Aggregate(new StringBuilder(), (s, r) => s.AppendLine(r.Message)).ToString();
                        topsToGoErrors += " - " + errors;
                    }
                }
            }
        }

        private void UpdateProject44(string assetId, string loadId, string token, ref string project44Errors)
        {
            const string mediaType = "application/json";
            string url = _config.GetValue<string>("VisibilityApiAddress") + "asset/associate-asset-id";

            var formData = JsonConvert.SerializeObject(new AssociateAssetRequest { AssetId = assetId, LoadId = loadId });
            var content = new StringContent(formData, Encoding.UTF8, mediaType);

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", token);

            var responseMessage = _client.PostAsync(url, content, default(CancellationToken)).Result;
            var responseString = responseMessage.Content.ReadAsStringAsync().Result.Replace("\"code\":null,", "\"code\":0,");
            var response = JsonConvert.DeserializeObject<ResponseMessage<AssociateAssetRequest>>(responseString);

            if (!responseMessage.IsSuccessStatusCode || response == null || response.Errors.Count > 0 || (response.Data != null && response.Data.UpdateSuccess == false))
            {
                project44Errors = "Truck number save failed";

                if (response != null && response.Errors.Count > 0)
                {
                    var errors = response.Errors.Aggregate(new StringBuilder(), (s, r) => s.AppendLine(r.Message)).ToString();
                    project44Errors += " - " + errors;
                }
            }
        }

        private LoadClaimEntity GetLatestLoadClaim(Guid loadId)
        {
            return _context.LoadClaims
                                .Include(lc => lc.CarrierScac)
                                .Where(lc => lc.Transaction.LoadId == loadId)
                                .OrderByDescending(lc => lc.CreateDtTm)
                                .FirstOrDefault();
        }
    }
}