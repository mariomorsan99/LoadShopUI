using AutoMapper;
using Loadshop.Data;
using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Loadshop.Services.Data.CarrierWebAPI;
using Loadshop.DomainServices.Security;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class LoadStatusTransactionService : ILoadStatusTransactionService
    {
        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly ISecurityService _securityService;
        private readonly ICarrierWebAPIService _carrierWebAPIService;
        private readonly int AdjustForFutureTimeThresholdMinutes = 3;

        public LoadStatusTransactionService(
            LoadshopDataContext context,
            IMapper mapper,
            IUserContext userContext,
            ISecurityService securityService,
            ICarrierWebAPIService carrierWebAPIService)
        {
            _context = context;
            _mapper = mapper;
            _userContext = userContext;
            _securityService = securityService;
            _carrierWebAPIService = carrierWebAPIService;

        }

        public async Task<LoadStatusTransactionData> GetLatestStatus(Guid loadId)
        {
            await _securityService.GuardActionAsync(SecurityActions.Loadshop_Ui_My_Loads_Status_View, SecurityActions.Loadshop_Ui_Shopit_Load_View_Booked_Detail);

            var (load, scac) = await GetLoadAndScac(loadId, includeStops: false);

            var status = await _context.LoadStatusTransactions
                .Where(_ => _.LoadId == loadId)
                .OrderByDescending(_ => _.TransactionDtTm)
                .FirstOrDefaultAsync();

            return status != null ? _mapper.Map<LoadStatusTransactionData>(status) : null;
        }

        public async Task<BaseServiceResponse> AddInTransitStatus(LoadStatusInTransitData inTransitData)
        {
            await _securityService.GuardActionAsync(SecurityActions.Loadshop_Ui_My_Loads_Status_Update);

            var (load, scac) = await GetLoadAndScac(inTransitData.LoadId, includeStops: false );

            //Adjust the time if it's close to current
            bool inFuture;
            (inTransitData.LocationTime, inFuture) = AdjustLocationTime(inTransitData.LocationTime, inTransitData.Latitude, inTransitData.Longitude);

            var response = new BaseServiceResponse();
            if (inTransitData.LocationTime == null)
            {
                response.AddError($"urn:root:{nameof(LoadStatusInTransitData.LocationTime)}", "Status Date/Time is required");
            }
            else if (inFuture)
            {
                response.AddError($"urn:root:{nameof(LoadStatusInTransitData.LocationTime)}", "Status Date/Time must be in the past");
            }

            if (inTransitData.Latitude == null)
            {
                response.AddError($"urn:root:{nameof(LoadStatusInTransitData.Latitude)}", "Location is required");
            }
            else if(inTransitData.Longitude == null)
            {
                response.AddError($"urn:root:{nameof(LoadStatusInTransitData.Longitude)}", "Location is required");
            }

            if (!response.IsSuccess)
                return response;

            var currentTime = DateTimeOffset.UtcNow;
            var lst = new LoadStatusTransactionEntity
            {
                LoadId = load.LoadId,
                TransactionDtTm = currentTime.DateTime//this may not be needed anymore if we can send the MessageTime
            };

            var eventResponse = await _carrierWebAPIService.Send(new LoadStatusEvent<InTransitLoadData>
            {
                MessageType = "LoadLocation",
                MessageId = Guid.NewGuid(),
                MessageTime = DateTimeOffset.Now,
                ApiVersion = "1.1",
                Payload = new InTransitLoadData
                {
                    Loads = new InTransitEventData[]
                    {
                        new InTransitEventData
                        {
                            LoadNumber = load.LatestLoadTransaction.Claim.BillingLoadId ?? load.PlatformPlusLoadId ?? load.ReferenceLoadId,
                            Latitude = inTransitData.Latitude.Value,
                            Longitude = inTransitData.Longitude.Value,
                            LocationTime = inTransitData.LocationTime?.ToString("yyyy-MM-ddTHH:mm:ss"),
                            IsLocalTime = true, //Always true if in local status time
                            Scac = scac
                        }
                    }
                }
            });

            lst.MessageId = eventResponse.MessageId;
            lst.MessageTime = eventResponse.MessageTime;
            _context.LoadStatusTransactions.Add(lst);
            await _context.SaveChangesAsync(_userContext.UserName);

            return response;
        }

        public async Task<BaseServiceResponse> AddStopStatuses(LoadStatusStopData stopData)
        {
            await _securityService.GuardActionAsync(SecurityActions.Loadshop_Ui_My_Loads_Status_Update);

            var (load, scac) = await GetLoadAndScac(stopData.LoadId, includeStops: true);

            var response = new BaseServiceResponse();
            if (stopData.Events?.Any() != true)
            {
                response.AddError($"urn:root:{nameof(LoadStatusStopData.Events)}:0:{nameof(LoadStatusStopEventData.EventTime)}", "Status Date/Time is required");
            }

            for (int i = 0; i < stopData.Events.Count; i++)
            {
                var stopEvent = stopData.Events[i];

                var stop = load.LoadStops?.FirstOrDefault(_ => _.StopNbr == stopEvent.StopNumber);
                var inFuture = false;

                if (stop == null)
                {
                    response.AddError($"urn:root:{nameof(LoadStatusStopData.Events)}:{i}", "Stop number does not exist");
                }
                else
                {
                    //Adjust the time if it's close to current
                    (stopEvent.EventTime, inFuture) = AdjustLocationTime(stopData.Events[i].EventTime, stop?.Latitude, stop?.Longitude);

                    if (stopEvent.EventTime == null)
                    {
                        response.AddError($"urn:root:{nameof(LoadStatusStopData.Events)}:{i}:{nameof(LoadStatusStopEventData.EventTime)}", "Status Date/Time is required");
                    }
                    else if (inFuture)
                    {
                        response.AddError($"urn:root:{nameof(LoadStatusStopData.Events)}:{i}:{nameof(LoadStatusStopEventData.EventTime)}", "Status Date/Time must be in the past");
                    }
                }
            }

            if (!response.IsSuccess)
                return response;


            foreach (var stopEvent in stopData.Events)
            {
                var currentTime = DateTimeOffset.UtcNow;
                var lst = new LoadStatusTransactionEntity
                {
                    LoadId = load.LoadId,
                    TransactionDtTm = currentTime.DateTime//this may not be needed anymore if we can send the MessageTime
                };

                var eventResponse = await _carrierWebAPIService.Send(new LoadStatusEvent<StopEventData>
                {
                    MessageType = "LoadStopEvent",
                    MessageId = Guid.NewGuid(),
                    MessageTime = DateTimeOffset.Now,
                    ApiVersion = "1.1",
                    Payload = new StopEventData
                    {
                        LoadNumber = load.LatestLoadTransaction.Claim.BillingLoadId ?? load.PlatformPlusLoadId ?? load.ReferenceLoadId,
                        StopNbr = stopEvent.StopNumber,
                        StatusDateTime = stopEvent.EventTime?.ToString("yyyy-MM-ddTHH:mm:ss"),
                        StatusType = stopEvent.EventType.ToString(),
                        IsLocalTime = true, //Always true if in local status time
                        Scac = scac
                    }
                });

                lst.MessageId = eventResponse.MessageId;
                lst.MessageTime = eventResponse.MessageTime;
                _context.LoadStatusTransactions.Add(lst);
                await _context.SaveChangesAsync(_userContext.UserName);
            }

            await AddInTransitStopStatus(stopData, load);

            return new BaseServiceResponse();
        }

        /// <summary>
        /// Adds in transit status if most recent event was a departure
        /// </summary>
        /// <param name="stopData"></param>
        /// <param name="load"></param>
        /// <returns></returns>
        public async Task<LoadStatusInTransitData> AddInTransitStopStatus(LoadStatusStopData stopData, LoadEntity load)
        {
            var mostRecentStop = stopData?.Events?.OrderBy(x => x.StopNumber).LastOrDefault();
            if (mostRecentStop?.EventType == StopEventTypeEnum.Departure)
            {
                var mostRecentLoadStop = load?.LoadStops?.FirstOrDefault(_ => _.StopNbr == mostRecentStop.StopNumber);
                if (mostRecentLoadStop != null)
                {
                    var inTransitStopData = new LoadStatusInTransitData()
                    {
                        LoadId = load.LoadId,
                        LocationTime = mostRecentStop.EventTime,
                        // https://kbxltrans.visualstudio.com/Suite/_workitems/edit/40298
                        Latitude = mostRecentLoadStop.Latitude,
                        Longitude = mostRecentLoadStop.Longitude
                    };

                    await AddInTransitStatus(inTransitStopData);

                    return inTransitStopData;
                }
            }

            return null;
        }

        private async Task<(LoadEntity, string)> GetLoadAndScac(Guid loadId, bool includeStops)
        {
            var query = _context.Loads
                .Include(_ => _.LatestLoadTransaction)
                .ThenInclude(_ => _.Claim)
                .AsQueryable();

            if (includeStops)
                query = query.Include(_ => _.LoadStops);

            var load = await query.SingleOrDefaultAsync(x => x.LoadId == loadId);
            if (load == null)
                throw new ValidationException("Load not found");

            var transactionTypeId = load?.LatestLoadTransaction?.TransactionTypeId;
            var scac = load?.LatestLoadTransaction?.Claim?.Scac;

            if (transactionTypeId != TransactionTypes.Accepted)
                throw new ValidationException("Load not accepted");

            if (scac == null)
                throw new ValidationException("Load not found");

            var user = await _context.Users.FirstOrDefaultAsync(_ => _.IdentUserId == _userContext.UserId);
            if (user == null)
                throw new ValidationException("Load not found");

            var isCarrier = user.PrimaryScac != null;
            if (isCarrier && scac != user.PrimaryScac)
                throw new ValidationException("Load not found");
            else if (!isCarrier && !(await _securityService.IsAuthorizedForCustomerAsync(load.CustomerId)))
                throw new ValidationException("Load not found");

            return (load, scac);
        }

        private (DateTimeOffset?, bool) AdjustLocationTime(DateTimeOffset? dt, decimal? latitude, decimal? longitude)
        {
            bool inFuture = false;
            if (dt == null || latitude == null || longitude == null)
                return (null, inFuture);

            var timeZone = GetTimeZoneFromLatLong(Decimal.ToDouble(latitude.Value), Decimal.ToDouble(longitude.Value));

            var statusLocationTime = new DateTimeOffset(dt.Value.DateTime, timeZone.BaseUtcOffset);

            var currentLocationDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(timeZone.StandardName));
            var currentLocationTime = new DateTimeOffset(currentLocationDateTime, timeZone.BaseUtcOffset);
            currentLocationTime = currentLocationTime.AddMilliseconds(-currentLocationTime.Millisecond).AddSeconds(-currentLocationTime.Second);

            var minutesInFuture = statusLocationTime.Subtract(currentLocationTime).TotalMinutes;
            inFuture = minutesInFuture > 0;
            if (minutesInFuture > 0 && minutesInFuture <= AdjustForFutureTimeThresholdMinutes)
                return (currentLocationTime, false);
            return (statusLocationTime, inFuture);
        }

        private TimeZoneInfo GetTimeZoneFromLatLong(double lat, double lng)
        {
            var ianaTZ = GeoTimeZone.TimeZoneLookup.GetTimeZone(lat, lng)?.Result;
            if (string.IsNullOrWhiteSpace(ianaTZ))
                throw new Exception("Unable to determine time zone for load stop");

            var tz = TimeZoneConverter.TZConvert.GetTimeZoneInfo(ianaTZ);
            if (tz == null)
                throw new Exception("Unable to convert time zone from IANA to Windows");
            return tz;
        }
    }
}
