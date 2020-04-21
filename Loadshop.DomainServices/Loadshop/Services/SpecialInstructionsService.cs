using AutoMapper;
using Ganss.XSS;
using Loadshop.DomainServices.Common.Services;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Loadshop.DomainServices.Validation.Services;

namespace Loadshop.DomainServices.Loadshop.Services
{
    public class SpecialInstructionsService : ISpecialInstructionsService
    {
        private const string ErrorPrefix = "urn:SpecialInstruction";

        private readonly LoadshopDataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;
        private readonly ISecurityService _securityService;
        private readonly ICommonService _commonService;
        private readonly IHtmlSanitizer _htmlSanitizer;

        public SpecialInstructionsService(LoadshopDataContext context,
            IMapper mapper,
            IUserContext userContext,
            ISecurityService securityService,
            ICommonService commonService,
            IHtmlSanitizer htmlSanitizer)
        {
            _context = context;
            _mapper = mapper;
            _userContext = userContext;
            _securityService = securityService;
            _commonService = commonService;
            _htmlSanitizer = htmlSanitizer;
        }

        public async Task<List<SpecialInstructionData>> GetSpecialInstructionsAsync()
        {
            var userPrimaryCustomerId = _context.Users.SingleOrDefault(u => u.IdentUserId == _userContext.UserId)?.PrimaryCustomerId;

            var entities = await _context.SpecialInstructions.AsNoTracking()
                .Include(x => x.SpecialInstructionEquipment)
                .ThenInclude(x => x.Equipment)
                .Where(x => x.CustomerId == userPrimaryCustomerId)
                .ToListAsync();

            var result = _mapper.Map<List<SpecialInstructionData>>(entities);

            result.ForEach(x => x.Comments = _htmlSanitizer.Sanitize(x.Comments));

            return result;
        }

        public async Task<SpecialInstructionData> GetSpecialInstructionAsync(long specialInstructionsId)
        {
            var entity = await _context.SpecialInstructions.AsNoTracking()
                .Include(x => x.SpecialInstructionEquipment)
                .ThenInclude(x => x.Equipment)
                .Where(x => x.SpecialInstructionId == specialInstructionsId)
                .SingleOrDefaultAsync();
            if (entity == null)
                throw new Exception("Special Instructions not found.");

            GuardCustomer(entity.CustomerId);

            var instruction = _mapper.Map<SpecialInstructionData>(entity);

            instruction.Comments = _htmlSanitizer.Sanitize(instruction.Comments);

            return instruction;
        }

        public async Task<SaveSpecialInstructionResponse> CreateSpecialInstructionAsync(SpecialInstructionData instruction, string username)
        {
            var response = new SaveSpecialInstructionResponse();
            if (instruction == null)
            {
                response.ModelState.AddModelError($"{ErrorPrefix}", "Special Instruction is requred");
                return response;
            }

            ConvertStatesToAbbreviations(instruction);
            if (instruction.SpecialInstructionId > 0)
            {
                response.ModelState.AddModelError($"{ErrorPrefix}", "Special Instruction should not have an Id assigned when creating.");
                return response;
            }

            var validationErrorMessage = ValidateSpecialInstruction(instruction);
            if (!string.IsNullOrWhiteSpace(validationErrorMessage))
            {
                response.ModelState.AddModelError($"{ErrorPrefix}", validationErrorMessage);
                return response;
            }

            instruction.Comments = _htmlSanitizer.Sanitize(instruction.Comments);

            var dbGroup = _mapper.Map<SpecialInstructionEntity>(instruction);

            //Map Equipment Types
            dbGroup.SpecialInstructionEquipment = new List<SpecialInstructionEquipmentEntity>();
            dbGroup.SpecialInstructionEquipment.MapList(instruction.SpecialInstructionEquipment, lcgeEntity => lcgeEntity.SpecialInstructionEquipmentId, lcgeData => lcgeData.SpecialInstructionEquipmentId, _mapper);

            GuardCustomer(dbGroup.CustomerId);

            var dup = await CheckIfDuplicateExists(dbGroup);

            if (dup)
            {
                response.ModelState.AddModelError($"{ErrorPrefix}", GetSpecialInstructionsDupErrorMessage(instruction));
                return response;
            }

            _context.SpecialInstructions.Add(dbGroup);
            await _context.SaveChangesAsync(username);
            response.SpecialInstructionData = await GetSpecialInstructionAsync(dbGroup.SpecialInstructionId);
            return response;
        }

        public async Task DeleteSpecialInstructionAsync(long id)
        {
            var dbGroup = await _context.SpecialInstructions.SingleOrDefaultAsync(x => x.SpecialInstructionId == id);
            if (dbGroup == null)
            {
                throw new Exception($"Special Instruction not found in the database with Id: {id}");
            }

            GuardCustomer(dbGroup.CustomerId);

            // remove all equipment
            var equipment = await _context.SpecialInstructionEquipments.Where(x => x.SpecialInstructionId == id).ToListAsync();
            _context.SpecialInstructionEquipments.RemoveRange(equipment);

            _context.SpecialInstructions.Remove(dbGroup);
            await _context.SaveChangesAsync();
        }

        public async Task<SaveSpecialInstructionResponse> UpdateSpecialInstructionAsync(SpecialInstructionData instruction, string username)
        {
            var response = new SaveSpecialInstructionResponse();
            ConvertStatesToAbbreviations(instruction);

            if (instruction == null || instruction.SpecialInstructionId <= 0)
            {
                response.ModelState.AddModelError($"{ErrorPrefix}", "Special Instruction should have an Id assigned when updating.");
                return response;
            }

            var validationErrorMessage = ValidateSpecialInstruction(instruction);
            if (!string.IsNullOrWhiteSpace(validationErrorMessage))
            {
                response.ModelState.AddModelError($"{ErrorPrefix}", validationErrorMessage);
                return response;
            }

            var dbGroup = await _context.SpecialInstructions
                            .Include(x => x.SpecialInstructionEquipment)
                            .Where(x => x.SpecialInstructionId == instruction.SpecialInstructionId)
                            .SingleOrDefaultAsync();
            if (dbGroup == null)
            {
                response.ModelState.AddModelError($"{ErrorPrefix}", "Special Instruction not found");
                return response;
            }

            GuardCustomer(dbGroup.CustomerId);

            instruction.Comments = _htmlSanitizer.Sanitize(instruction.Comments);

            _mapper.Map(instruction, dbGroup);

            //Update Load Carrier Group Equipment Entity
            if (dbGroup.SpecialInstructionEquipment != null)
            {
                dbGroup.SpecialInstructionEquipment.MapList(
                    instruction.SpecialInstructionEquipment,
                    lcgeEntity => lcgeEntity.SpecialInstructionEquipmentId,
                    legeData => legeData.SpecialInstructionEquipmentId,
                    _mapper);
            }

            var dup = await CheckIfDuplicateExists(dbGroup);

            if (dup)
            {
                response.ModelState.AddModelError($"{ErrorPrefix}", GetSpecialInstructionsDupErrorMessage(instruction));
                return response;
            }

            await _context.SaveChangesAsync(username);

            response.SpecialInstructionData = await GetSpecialInstructionAsync(dbGroup.SpecialInstructionId);
            return response;
        }

        private async Task<bool> CheckIfDuplicateExists(SpecialInstructionEntity instructionEntity)
        {
            // check if dups exist
            var dup = await _context.SpecialInstructions.Include(x => x.SpecialInstructionEquipment)
                            .Where(x => x.CustomerId == instructionEntity.CustomerId)
                            .Where(x => x.SpecialInstructionId != instructionEntity.SpecialInstructionId
                                    && x.OriginAddress1 == instructionEntity.OriginAddress1
                                    && x.OriginCity == instructionEntity.OriginCity
                                    && x.OriginCountry == instructionEntity.OriginCountry
                                    && x.OriginPostalCode == instructionEntity.OriginPostalCode
                                    && x.OriginState == instructionEntity.OriginState
                                    && x.DestinationAddress1 == instructionEntity.DestinationAddress1
                                    && x.DestinationCity == instructionEntity.DestinationCity
                                    && x.DestinationCountry == instructionEntity.DestinationCountry
                                    && x.DestinationPostalCode == instructionEntity.DestinationPostalCode
                                    && x.DestinationState == instructionEntity.DestinationState)
                            .Where(x => instructionEntity.SpecialInstructionEquipment.All(y => x.SpecialInstructionEquipment.Any(z => y.EquipmentId == z.EquipmentId)))
                            .AnyAsync();

            return dup;
        }

        public async Task<List<SpecialInstructionData>> GetSpecialInstructionsForLoadAsync(LoadEntity load)
        {
            if (load == null)
            {
                throw new Exception($"Load cannot be null");
            }

            GuardCustomer(load.CustomerId);

            var orderedStops = load.LoadStops?.OrderBy(_ => _.StopNbr).ToList();
            var origin = orderedStops?.FirstOrDefault();
            var destination = orderedStops?.LastOrDefault();

            if (origin == null || destination == null)
                return new List<SpecialInstructionData>();

            var matchingGroups = await _context.SpecialInstructions.AsNoTracking()
                .Include(x => x.SpecialInstructionEquipment)
                .Where(_ => _.CustomerId == load.CustomerId
                    && (_.OriginCity == null || _.OriginCity == origin.City)
                    && (_.OriginState == null || _.OriginState == origin.State)
                    && (_.OriginPostalCode == null || _.OriginPostalCode == origin.PostalCode)
                    && (_.OriginCountry == null || _.OriginCountry == origin.Country)
                    && (_.DestinationCity == null || _.DestinationCity == destination.City)
                    && (_.DestinationState == null || _.DestinationState == destination.State)
                    && (_.DestinationPostalCode == null || _.DestinationPostalCode == destination.PostalCode)
                    && (_.DestinationCountry == null || _.DestinationCountry == destination.Country)
                    && (!_.SpecialInstructionEquipment.Any() || _.SpecialInstructionEquipment.Any(equipment => equipment.EquipmentId == load.EquipmentId))
                    )
                .ToListAsync();

            // Address standardization has to happen after initial query
            matchingGroups = matchingGroups.Where(_ =>
                (_.OriginAddress1 == null || AddressValidationService.StandardizeAddress(origin.Address1, true)
                    .Equals(AddressValidationService.StandardizeAddress(_.OriginAddress1, true), StringComparison.OrdinalIgnoreCase))
                && (_.DestinationAddress1 == null || AddressValidationService.StandardizeAddress(destination.Address1, true)
                    .Equals(AddressValidationService.StandardizeAddress(_.DestinationAddress1, true), StringComparison.OrdinalIgnoreCase))
            ).ToList();

            return RankInstructions(matchingGroups);
        }

        private List<SpecialInstructionData> RankInstructions(IEnumerable<SpecialInstructionEntity> instructionEntities)
        {
            var results = new List<SpecialInstructionData>();

            // rank the instructions, orig -> dest trumps orig only / dest only
            var rankedInstructions = instructionEntities.Select(_ => new
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
                   + ((_.SpecialInstructionEquipment?.Count != null && _.SpecialInstructionEquipment?.Count > 0) ? 1 : 0)
            }).ToList();
            
            var maxRank = rankedInstructions.Select(_ => _.Rank).DefaultIfEmpty(0).Max();
            if (maxRank > 0)
            {
                var instructions = rankedInstructions.Where(_ => _.Rank == maxRank).Select(_ => _.Group).ToList();
                results.AddRange(_mapper.Map<List<SpecialInstructionData>>(instructions));
            }
            
            if (!results.Any())
            {
                results = _mapper.Map<List<SpecialInstructionData>>(instructionEntities);
            }

            //alternate form of Distinct() since we are working on objects
            return results.GroupBy(_ => _.SpecialInstructionId).Select(group => group.First()).ToList();
        }

        private void ConvertStatesToAbbreviations(SpecialInstructionData instructionData)
        {
            if (instructionData != null)
            {
                instructionData.OriginState = ConvertStateToAbbreviation(instructionData.OriginState);
                instructionData.DestinationState = ConvertStateToAbbreviation(instructionData.DestinationState);
            }
        }

        private string ConvertStateToAbbreviation(string stateName)
        {
            if (!string.IsNullOrWhiteSpace(stateName))
            {
                var state = _commonService.GetUSCANStateProvince(stateName);
                if (state != null)
                {
                    stateName = state.Abbreviation;
                }
            }

            return stateName;
        }

        private void GuardCustomer(Guid customerId)
        {
            if (!_securityService.IsAuthorizedForCustomer(customerId))
                throw new UnauthorizedAccessException($"User is not authorized for customer: {customerId}");
        }

        private static string GetSpecialInstructionsDupErrorMessage(SpecialInstructionData group)
        {
            var msg = new StringBuilder("A special instruction already exists for:" + Environment.NewLine);
            if (!string.IsNullOrWhiteSpace(group.OriginAddress1)) msg.Append($"Origin Address1 - {group.OriginAddress1}{Environment.NewLine}");
            if (!string.IsNullOrWhiteSpace(group.OriginCity)) msg.Append($"Origin City - {group.OriginCity}{Environment.NewLine}");
            if (!string.IsNullOrWhiteSpace(group.OriginState)) msg.Append($"Origin State - {group.OriginState}{Environment.NewLine}");
            if (!string.IsNullOrWhiteSpace(group.OriginPostalCode)) msg.Append($"Origin Postal Code - {group.OriginPostalCode}{Environment.NewLine}");
            if (!string.IsNullOrWhiteSpace(group.OriginCountry)) msg.Append($"Origin Country - {group.OriginCountry}{Environment.NewLine}");

            if (!string.IsNullOrWhiteSpace(group.DestinationAddress1)) msg.Append($"Destination Address1 - {group.DestinationAddress1}{Environment.NewLine}");
            if (!string.IsNullOrWhiteSpace(group.DestinationCity)) msg.Append($"Destination City - {group.DestinationCity}{Environment.NewLine}");
            if (!string.IsNullOrWhiteSpace(group.DestinationState)) msg.Append($"Destination State - {group.DestinationState}{Environment.NewLine}");
            if (!string.IsNullOrWhiteSpace(group.DestinationPostalCode)) msg.Append($"Destination Postal Code - {group.DestinationPostalCode}{Environment.NewLine}");
            if (!string.IsNullOrWhiteSpace(group.DestinationCountry)) msg.Append($"Destination Country - {group.DestinationCountry}{Environment.NewLine}");

            if (group.SpecialInstructionEquipment.Any()) msg.Append($"Equipment Type(s) - {string.Join(", ", group.SpecialInstructionEquipment.Select(lcge => lcge.EquipmentId))}{Environment.NewLine}");
            return msg.ToString();
        }

        private static string ValidateSpecialInstruction(SpecialInstructionData instructionData)
        {
            var errors = new StringBuilder();

            if (instructionData.CustomerId == Guid.Empty)
                errors.AppendLine("Must have a Customer");

            if (string.IsNullOrWhiteSpace(instructionData.Name))
            {
                errors.AppendLine("Must have a name");
            }
            var validOrigin = ValidateLocation(instructionData.OriginCity, instructionData.OriginState, instructionData.OriginCountry);
            var validDest = ValidateLocation(instructionData.DestinationCity, instructionData.DestinationState, instructionData.DestinationCountry);
            var validEquipment = (instructionData.SpecialInstructionEquipment?.Any()).GetValueOrDefault();

            if (!validOrigin && !validDest && !validEquipment)
            {
                errors.AppendLine("Must have an Origin, Destination, or Equipment");
            }

            if (string.IsNullOrWhiteSpace(instructionData.Comments) ||
                instructionData.Comments.Equals("<p></p>")) // default text for quill editor
            {
                errors.AppendLine("Must have an instruction");
            }

            if (errors.Length > 0)
            {
                return errors.ToString();
            }
            return string.Empty;
        }

        private static bool ValidateLocation(string city, string state, string country)
        {
            if (string.IsNullOrWhiteSpace(city) && string.IsNullOrWhiteSpace(state) && string.IsNullOrWhiteSpace(country))
            {
                return false;
            }

            return true;
        }

    }
}
