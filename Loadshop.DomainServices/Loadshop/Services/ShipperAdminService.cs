using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loadshop.API.Models;
using Loadshop.API.Models.ViewModels;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Proxy.Tops.Loadshop;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class ShipperAdminService : IShipperAdminService
    {
        private readonly LoadshopDataContext _context;
        private readonly ISecurityService _securityService;
        private readonly IMapper _mapper;
        private readonly ITopsLoadshopApiService _topsLoadshopApiService;
        private readonly IUserAdminService _userAdminService;
        private readonly INotificationService _notificationService;
        private readonly IUserContext _userContext;

        public ShipperAdminService(LoadshopDataContext context, ISecurityService securityService, IMapper mapper,
            ITopsLoadshopApiService topsLoadshopApiService, IUserAdminService userAdminService,
            INotificationService notificationService, IUserContext userContext)
        {
            _context = context;            
            _securityService = securityService;
            _mapper = mapper;
            _topsLoadshopApiService = topsLoadshopApiService;
            _userAdminService = userAdminService;
            _notificationService = notificationService;
            _userContext = userContext;
        }

        public List<CustomerData> GetAllShippers()
        {
            var shippers = _context.Customers.ToList();
            return _mapper.Map<List<CustomerData>>(shippers);
        }

        #region Maintain Shipper
        public CustomerProfileData GetShipper(Guid customerId)
        {
            var shipper = GetDbShipper(customerId);
            return _mapper.Map<CustomerProfileData>(shipper);
        }

        private CustomerEntity GetDbShipper(Guid customerId)
        {
            return _context.Customers
                .Include(x => x.CustomerCarrierScacContracts)
                .Include(x => x.CustomerContacts)
                .SingleOrDefault(x => x.CustomerId == customerId);
        }

        public CustomerProfileData AddShipper(CustomerProfileData customer, string username)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_System_Shipper_Add_Edit);

            if (customer != null && !customer.CustomerLoadTypeId.HasValue)
                customer.CustomerLoadTypeExpirationDate = null;

            ValidateCustomerOrderType(customer);
            ValidateFeeStructure(customer);

            SendFeeChangeEmail(null, customer, "Test");

            var dbCustomer = _mapper.Map<CustomerEntity>(customer);
            dbCustomer.DataSource = "LOADSHOP";

            _context.Customers.Add(dbCustomer);

            if(customer.CustomerCarrierScacs.Any())
            {
                dbCustomer.CustomerCarrierScacContracts = new List<CustomerCarrierScacContractEntity>();
            }

            foreach (var scac in customer.CustomerCarrierScacs)
            {
                var dbCustomerCarrierScac = new CustomerCarrierScacContractEntity
                {
                    Scac = scac
                };
                dbCustomer.CustomerCarrierScacContracts.Add(dbCustomerCarrierScac);
            }

            _context.SaveChanges(username);
            return _mapper.Map<CustomerProfileData>(GetDbShipper(dbCustomer.CustomerId));
        }

        /// <summary>
        /// Validates customer order type
        /// </summary>
        /// <param name="customerProfile"></param>
        /// <returns></returns>
        public void ValidateCustomerOrderType(CustomerProfileData customerProfile)
        {
            if (customerProfile != null && customerProfile.CustomerLoadTypeId.HasValue)
            {
                if (customerProfile.CustomerLoadTypeId.Value == (int)CustomerLoadTypeEnum.NewShipper
                    && !customerProfile.CustomerLoadTypeExpirationDate.HasValue)
                {
                    throw new ValidationException("Expiration Date is required for New Shipper type.");
                }
            }
        }

        private void ValidateFeeStructure(CustomerProfileData customerProfile)
        {
            if (customerProfile != null)
            {
                if (customerProfile.InNetworkFlatFee < 0)
                {
                    throw new ValidationException("In Network Flat Fee must be $0.00 or more.");
                }
                if (customerProfile.InNetworkPercentFee < 0)
                {
                    throw new ValidationException("In Network Flat Fee must be 0.00% or more.");
                }
                if (customerProfile.InNetworkPercentFee >= 10)
                {
                    throw new ValidationException("In Network Flat Fee must be less than 1,000%.");
                }
                if (customerProfile.OutNetworkFlatFee < 0)
                {
                    throw new ValidationException("Out Network Flat Fee must be $0.00 or more.");
                }
                if (customerProfile.OutNetworkPercentFee < 0)
                {
                    throw new ValidationException("Out Network Flat Fee must be 0.00% or more.");
                }
                if (customerProfile.OutNetworkPercentFee >= 10)
                {
                    throw new ValidationException("Out Network Flat Fee must be less than 1,000%.");
                }
            }
        }

        public CustomerProfileData UpdateShipper(CustomerProfileData customer, string username)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_System_Shipper_Add_Edit);

            if (customer != null && !customer.CustomerLoadTypeId.HasValue)
                customer.CustomerLoadTypeExpirationDate = null;

            if (!customer.CustomerId.HasValue)
            {
                throw new Exception("Customer Id cannot be null");
            }

            ValidateCustomerOrderType(customer);
            ValidateFeeStructure(customer);

            var dbCustomer = GetDbShipper(customer.CustomerId.Value);

            SendFeeChangeEmail(dbCustomer, customer, "Test");

            dbCustomer.Name = customer.Name;
            dbCustomer.DefaultCommodity = customer.DefaultCommodity;
            dbCustomer.UseFuelRerating = customer.UseFuelRerating;
            dbCustomer.FuelReratingNumberOfDays = Math.Abs(customer.FuelReratingNumberOfDays.GetValueOrDefault(0));
            dbCustomer.AllowZeroFuel = customer.AllowZeroFuel;
            dbCustomer.AllowEditingFuel = customer.AllowEditingFuel;
            dbCustomer.TopsOwnerId = customer.TopsOwnerId;
            dbCustomer.SuccessManagerUserId = customer.SuccessManagerUserId;
            dbCustomer.SuccessSpecialistUserId = customer.SuccessSpecialistUserId;
            dbCustomer.Comments = customer.Comments;
            dbCustomer.CustomerLoadTypeId = customer.CustomerLoadTypeId;
            dbCustomer.CustomerLoadTypeExpirationDate = customer.CustomerLoadTypeExpirationDate;
            dbCustomer.AutoPostLoad = customer.AutoPostLoad;
            dbCustomer.ValidateUniqueReferenceLoadIds = customer.ValidateUniqueReferenceLoadIds;
            dbCustomer.AllowManualLoadCreation = customer.AllowManualLoadCreation;
            dbCustomer.InNetworkFlatFee = customer.InNetworkFlatFee;
            dbCustomer.InNetworkPercentFee = customer.InNetworkPercentFee;
            dbCustomer.OutNetworkFlatFee = customer.OutNetworkFlatFee;
            dbCustomer.OutNetworkPercentFee = customer.OutNetworkPercentFee;
            dbCustomer.InNetworkFeeAdd = customer.InNetworkFeeAdd;
            dbCustomer.OutNetworkFeeAdd = customer.OutNetworkFeeAdd;
            dbCustomer.RequireMarginalAnalysis = customer.RequireMarginalAnalysis;

            // Customer Contacts
            foreach (var customerContact in customer.CustomerContacts)
            {
                var dbCustomerContact = _context.CustomerContacts
                    .SingleOrDefault(x => x.CustomerContactId == customerContact.CustomerContactId);

                if (dbCustomerContact == null)
                {
                    dbCustomerContact = _mapper.Map<CustomerContactEntity>(customerContact);
                    dbCustomer.CustomerContacts.Add(dbCustomerContact);
                }
                else
                {
                    dbCustomerContact.FirstName = customerContact.FirstName;
                    dbCustomerContact.LastName = customerContact.LastName;
                    dbCustomerContact.Position = customerContact.Position;
                    dbCustomerContact.PhoneNumber = customerContact.PhoneNumber;
                    dbCustomerContact.Email = customerContact.Email;
                }
            }
            foreach (var dbCustomerContact in dbCustomer.CustomerContacts.Where(x => x.CustomerContactId != Guid.Empty))
            {
                if (customer.CustomerContacts.All(x => x.CustomerContactId != dbCustomerContact.CustomerContactId))
                {
                    _context.CustomerContacts.Remove(dbCustomerContact);
                }
            }

            //Only update if user has access
            if (_securityService.UserHasAction(SecurityActions.Loadshop_Ui_System_Shipper_Contracted_Carriers_Add_Edit))
            {
                // Customer Carrier Scacs
                var dbCustomerCarrierScacs = _context.CustomerCarrierScacContracts.Where(x => x.CustomerId == customer.CustomerId).ToList();
                foreach (var scac in customer.CustomerCarrierScacs)
                {
                    var dbCustomerCarrierScac = dbCustomerCarrierScacs.SingleOrDefault(x => x.Scac == scac);

                    if (dbCustomerCarrierScac == null)
                    {
                        dbCustomerCarrierScac = new CustomerCarrierScacContractEntity
                        {
                            Scac = scac
                        };
                        dbCustomerCarrierScacs.Add(dbCustomerCarrierScac);
                        dbCustomer.CustomerCarrierScacContracts.Add(dbCustomerCarrierScac);
                    }
                }
                foreach (var dbScac in dbCustomer.CustomerCarrierScacContracts.Where(x => x.CustomerCarrierContractId != Guid.Empty))
                {
                    if (customer.CustomerCarrierScacs.All(x => x != dbScac.Scac))
                    {
                        _context.CustomerCarrierScacContracts.Remove(dbScac);
                    }
                }
            }

            _context.SaveChanges(username);
            return _mapper.Map<CustomerProfileData>(GetDbShipper(dbCustomer.CustomerId));
        }

        private void SendFeeChangeEmail(CustomerEntity dbCustomer, CustomerProfileData customer, string changeUser)
        {
            if (dbCustomer == null
                || dbCustomer.InNetworkFlatFee != customer.InNetworkFlatFee
                || dbCustomer.InNetworkPercentFee != customer.InNetworkPercentFee
                || dbCustomer.InNetworkFeeAdd != customer.InNetworkFeeAdd
                || dbCustomer.OutNetworkFlatFee != customer.OutNetworkFlatFee
                || dbCustomer.OutNetworkPercentFee != customer.OutNetworkPercentFee
                || dbCustomer.OutNetworkFeeAdd != customer.OutNetworkFeeAdd
                )
            {
                _notificationService.SendShipperFeeChangeEmail(dbCustomer, customer, $"{_userContext.FirstName} {_userContext.LastName}");
            }
        }

        #endregion Maintain Shipper

        #region Maintain Shipper/Carrier Associations

        public List<CarrierScacData> GetCarriersForShipper(Guid customerId)
        {
            var shipperCarrierScacs = _context.CustomerCarrierScacContracts
                .Where(x => x.CustomerId == customerId)
                .Select(x => x.Scac)
                .ToList();

            var shipperCarriers = _context.CarrierScacs
                .Where(x => shipperCarrierScacs.Contains(x.Scac))
                .ToList();

            return _mapper.Map<List<CarrierScacData>>(shipperCarriers);
        }

        public List<CarrierScacData> GetAvailableCarriersForShipper(Guid customerId)
        {
            var currentDateTime = DateTime.Now;

            var currentShipperScacs = _context.CustomerCarrierScacContracts
                .Where(x => x.CustomerId == customerId)
                .Select(x => x.Scac)
                .ToList();

            var avaialableShipperCarriers = _context.CarrierScacs
                .Where(x => !currentShipperScacs.Contains(x.Scac)
                       && x.IsActive == true
                       && (x.EffectiveDate <= currentDateTime)
                       && (x.ExpirationDate == null || x.ExpirationDate > currentDateTime))
                .ToList();

            return _mapper.Map<List<CarrierScacData>>(avaialableShipperCarriers);
        }

        public CustomerCarrierScacContractData AddCarrierToShipper(Guid customerId, string scac, Guid userId)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_System_Shipper_Contracted_Carriers_Add_Edit);

            var customerCarrierScacContractEntity = new CustomerCarrierScacContractEntity
            {
                CustomerId = customerId,
                Scac = scac
            };

            var user = _context.Users.Where(x => x.IdentUserId == userId).FirstOrDefault();

            _context.CustomerCarrierScacContracts.Add(customerCarrierScacContractEntity);
            _context.SaveChanges(user.Username);
            return _mapper.Map<CustomerCarrierScacContractData>(customerCarrierScacContractEntity);
        }

        public List<CustomerCarrierScacContractData> AddCarriersToShipper(Guid customerId, List<string> scacs, Guid userId)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_System_Shipper_Contracted_Carriers_Add_Edit);

            var user = _context.Users.Where(x => x.IdentUserId == userId).FirstOrDefault();

            var customerCarrierScacContractEntities = new List<CustomerCarrierScacContractEntity>();

            foreach (var scac in scacs)
            {
                var customerCarrierScacContractEntity = new CustomerCarrierScacContractEntity
                {
                    CustomerId = customerId,
                    Scac = scac
                };

                customerCarrierScacContractEntities.Add(customerCarrierScacContractEntity);
            }

            _context.CustomerCarrierScacContracts.AddRange(customerCarrierScacContractEntities);
            _context.SaveChanges(user.Username);
            return _mapper.Map<List<CustomerCarrierScacContractData>>(customerCarrierScacContractEntities);
        }

        public bool DeleteCarrierFromShipper(Guid customerId, Guid customerCarrierContractId)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_System_Shipper_Contracted_Carriers_Add_Edit);

            var carrier = _context.CustomerCarrierScacContracts.SingleOrDefault(x => x.CustomerId == customerId && x.CustomerCarrierContractId == customerCarrierContractId);

            if (carrier == null)
            {
                throw new Exception($"CustomerCarrierContractId: {customerCarrierContractId} not found in the database for Customer: {customerId}");
            }

            _context.CustomerCarrierScacContracts.Remove(carrier);
            _context.SaveChanges();
            return true;
        }

        public bool DeleteCarriersFromShipper(Guid customerId, List<Guid> customerCarrierContractIds)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_System_Shipper_Contracted_Carriers_Add_Edit);

            var carriers = _context.CustomerCarrierScacContracts.Where(x => x.CustomerId == customerId && customerCarrierContractIds.Contains(x.CustomerCarrierContractId)).ToList();

            if (carriers == null)
            {
                throw new Exception($"None of the specified CustomerCarrierContractIds were found in the database for Customer: {customerId}. Specified CustomerCarrierContractIds: {string.Join(", ", customerCarrierContractIds.ToArray())}");
            }

            _context.CustomerCarrierScacContracts.RemoveRange(carriers);
            _context.SaveChanges();
            return true;
        }

        public bool DeleteAllCarriersFromShipper(Guid customerId)
        {
            _securityService.GuardAction(SecurityActions.Loadshop_Ui_System_Shipper_Contracted_Carriers_Add_Edit);

            var carriers = _context.CustomerCarrierScacContracts.Where(x => x.CustomerId == customerId).ToList();

            if (carriers == null)
            {
                throw new Exception($"No SCACs found in the database for CusomerId: {customerId}");
            }

            _context.CustomerCarrierScacContracts.RemoveRange(carriers);
            _context.SaveChanges();
            return true;
        }

        #endregion Maintain Shipper/Carrier Associations

        #region Maintain Shipper/User Associations

        public List<UserProfileData> GetUsersForShipper(Guid customerId)
        {
            var currentShipperUsers = _context.UserShippers
                .Where(x => x.CustomerId == customerId)
                .Select(x => x.UserId)
                .ToList();

            var users = _context.Users
                .Where(x => currentShipperUsers.Contains(x.UserId))
                .ToList();

            return _mapper.Map<List<UserProfileData>>(users);
        }

        public List<UserProfileData> GetAvailableUsersForShipper(Guid customerId)
        {
            var currentShipperUsers = _context.UserShippers
                .Where(x => x.CustomerId == customerId)
                .Select(x => x.UserId)
                .ToList();

            var avaialableShipperUsers = _context.Users
                .Where(x => x.UserId != customerId && !currentShipperUsers.Contains(x.UserId))
                .ToList();

            return _mapper.Map<List<UserProfileData>>(avaialableShipperUsers);
        }

        public UserShipperData AddUserToShipper(Guid customerId, Guid userId, Guid identUserId)
        {
            var user = _context.Users.Where(x => x.IdentUserId == identUserId).FirstOrDefault();

            var userShipper = new UserShipperEntity {
                CustomerId = customerId,
                UserId = userId
            };


            _context.UserShippers.Add(userShipper);
            _context.SaveChanges(user.Username);
            return _mapper.Map<UserShipperData>(userShipper);
        }

        public List<UserShipperData> AddUsersToShipper(Guid customerId, List<Guid> userIds, Guid identUserId)
        {
            var user = _context.Users.Where(x => x.IdentUserId == identUserId).FirstOrDefault();

            var userShippers = new List<UserShipperEntity>();

            foreach (var userId in userIds)
            {
                var userShipper = new UserShipperEntity
                {
                    CustomerId = customerId,
                    UserId = userId
                };

                userShippers.Add(userShipper);
            }

            _context.UserShippers.AddRange(userShippers);
            _context.SaveChanges(user.Username);
            return _mapper.Map<List<UserShipperData>>(userShippers);
        }

        public bool DeleteUserFromShipper(Guid customerId, Guid userId)
        {
            var user = _context.UserShippers.SingleOrDefault(x => x.CustomerId == customerId && x.UserId == userId);

            if (user == null)
            {
                throw new Exception($"UserId: {userId} not found in the database for Customer: {customerId}");
            }

            _context.UserShippers.Remove(user);
            _context.SaveChanges();
            return true;
        }

        public bool DeleteUsersFromShipper(Guid customerId, List<Guid> userIds)
        {
            var users = _context.UserShippers.Where(x => x.CustomerId == customerId && userIds.Contains(x.UserId)).ToList();

            if (users == null)
            {
                throw new Exception($"None of the specified UserIds were found in the database for Customer: {customerId}. Specified UserIds: {string.Join(", ", userIds.ToArray())}");
            }

            _context.UserShippers.RemoveRange(users);
            _context.SaveChanges();
            return true;
        }

        public bool DeleteAllUsersFromShipper(Guid customerId)
        {
            var users = _context.UserShippers.Where(x => x.CustomerId == customerId).ToList();

            if (users == null)
            {
                throw new Exception($"No Users found in the database for CustomerId: {customerId}");
            }

            _context.UserShippers.RemoveRange(users);
            _context.SaveChanges();
            return true;
        }

        #endregion Maintain Shipper/User Associations

        #region Maintain Shipper Mappings

        public async Task<ResponseMessage<CustomerProfileData>> CreateCustomerUser(Guid customerId, string username)
        {
            var customer = GetDbShipper(customerId);

            if(string.IsNullOrWhiteSpace(customer.TopsOwnerId))
            {
                return new ResponseMessage<CustomerProfileData>
                {
                    Errors = new List<ResponseError>
                    {
                        new ResponseError
                        {
                            Message = "Owner Id must be saved."
                        }
                    }
                };
            }

            var ssos = await _topsLoadshopApiService.GetSourceSystemOwners(customer.TopsOwnerId);

            if (!ssos.Success || !ssos.Data.ContainsKey(customer.TopsOwnerId))
            {
                return new ResponseMessage<CustomerProfileData>
                {
                    Errors = new List<ResponseError>
                    {
                        new ResponseError
                        {
                            Message = $"This Owner Id ({customer.TopsOwnerId}) is not setup properly in tops."
                        }
                    }
                };
            }

            var user = new RegisterViewModel
            {
                OwnerId = customer.TopsOwnerId.ToLower(),
                UserId = $"LS{customer.TopsOwnerId.ToLower()}",
                Password = $"LS{customer.TopsOwnerId.ToLower()}00",
                ConfirmPassword = $"LS{customer.TopsOwnerId.ToLower()}00",
                Company = customer.Name,
                FirstName = customer.Name,
                LastName = "User",
                Email = "LoadshopCustomerUser@none.com"
            };

            //1. Create Identity User
            var topsResult = await _topsLoadshopApiService.CreateCustomerUser(user);

            if (topsResult.Success)
            {
                //2. set identUserId on customer
                customer.IdentUserId = topsResult.Data.Id;
                customer.LastChgBy = username;
                customer.LastChgDtTm = DateTime.Now;
                _context.SaveChanges();

                //3. Create[user] for [customer] with same identUserId
                //  a. same username as processSetting
                //  b. assign them access to the customer and give them system role
                var securityAccessRoles = _mapper.Map<List<SecurityAccessRoleData>>(
                    _context.SecurityAccessRoles
                        .Where(x => x.AccessRoleName == "System")
                        .ToList());

                var userData = new UserData
                {
                    //User Data
                    Username = user.UserId,
                    CompanyName = user.Company,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    //Identity Data
                    IdentUserId = topsResult.Data.Id.GetValueOrDefault(),
                    //Customer Data
                    PrimaryCustomerId = customerId,
                    ShipperIds = new List<Guid> { customerId },
                    //Security Data
                    SecurityRoleIds = securityAccessRoles.Select(x => x.AccessRoleId).ToList(),
                    //General Data
                    IsNotificationsEnabled = false,
                    CarrierScacs = new List<string>(),
                    UserNotifications = new List<UserNotificationData>(),
                    CreateBy = username,
                    CreateDtTm = DateTime.Now,
                    LastChgBy = username,
                    LastChgDtTm = DateTime.Now
                };

                var userDataResponse = await _userAdminService.Create(userData);
                return new ResponseMessage<CustomerProfileData>
                {
                    Data = GetShipper(customerId),
                    Errors = userDataResponse.Exceptions?.Select(x => new ResponseError
                    {
                        Message = x.Message,
                        StackTrace = x.StackTrace,
                        Data = x.Data
                    }).ToList()
                };
            }

            return new ResponseMessage<CustomerProfileData>
            {
                Errors = topsResult.Errors
            };
        }
        #endregion

    }
}
