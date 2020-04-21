using FluentAssertions;
using AutoMapper;
using Loadshop.DomainServices.Loadshop.DataProvider;
using Loadshop.DomainServices.Loadshop.DataProvider.Entities;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Loadshop.DomainServices.Security;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Loadshop.DomainServices.Constants;
using System.Linq;
using System.Threading;
using Loadshop.DomainServices.Exceptions;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class AgreementDocumentServiceTests : IClassFixture<TestFixture>
    {
        private readonly IMapper _mapper;
        private readonly Mock<IUserContext> _userContext;
        private Mock<LoadshopDataContext> _db;
        private Mock<ISecurityService> _securityService;
        private AgreementDocumentService _svc;
        private static Guid USER_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");

        private readonly List<AgreementDocumentEntity> AGREEMENTDOCUMENTS = new List<AgreementDocumentEntity>()
            {
                new AgreementDocumentEntity()
                {
                    AgreementDocumentId = Guid.NewGuid(),
                    AgreementType = AgreementDocumentTypes.TermsAndPrivacy,
                    AgreementActiveDtTm = DateTime.Now.AddDays(-1),
                },
                new AgreementDocumentEntity()
                {
                    AgreementDocumentId = Guid.NewGuid(),
                    AgreementType = AgreementDocumentTypes.TermsAndPrivacy,
                    AgreementActiveDtTm = DateTime.Now.AddDays(5),
                },
         };

        public AgreementDocumentServiceTests(TestFixture fixture)
        {
            _mapper = fixture.Mapper;
            _userContext = new Mock<IUserContext>();
            _db = new MockDbBuilder()
                .WithAgreementDocuments(AGREEMENTDOCUMENTS)
                .Build();

            _securityService = new Mock<ISecurityService>();
            _securityService.Setup(x => x.GetAuthorizedCustomersforUser()).Returns(new List<CustomerData>() { new CustomerData() });

        }

        [Fact]
        public async Task GetLatestAgreementDocument_Terms_ShouldReturnExpectedAgreement()
        {
            _svc = CreateService();

            var terms = await _svc.GetLatestAgreementDocument(AgreementDocumentTypes.TermsAndPrivacy);

            terms.AgreementDocumentId.Should().Be(AGREEMENTDOCUMENTS.First().AgreementDocumentId);
        }
        
        [Fact]
        public async Task HasUserAgreedToLatestTerms_ShouldReturn_False()
        {
            _svc = CreateService();
            var agreed = await _svc.HasUserAgreedToLatestTermsAndPrivacy(USER_ID);

            agreed.Should().BeFalse();
        }

        [Fact]
        public async Task HasUserAgreedToLatestTerms_ShouldReturn_True()
        {
            var agreements = new List<UserAgreementDocumentEntity>()
            {
                new UserAgreementDocumentEntity()
                {
                    UserId = USER_ID,
                    AgreementDocumentId = AGREEMENTDOCUMENTS.FirstOrDefault(x=> x.AgreementType == AgreementDocumentTypes.TermsAndPrivacy).AgreementDocumentId
                }
            };

            _db = new MockDbBuilder()
                .WithAgreementDocuments(AGREEMENTDOCUMENTS)
                    .WithUserAgreements(agreements)
                   .Build();
            _svc = CreateService();

            var agreed = await _svc.HasUserAgreedToLatestTermsAndPrivacy(USER_ID);

            agreed.Should().BeTrue();
        }


        [Fact]
        public async Task UserAgreement_ShouldReturn_True()
        {
            _userContext.SetupGet(x => x.UserId).Returns(USER_ID);
            _db = new MockDbBuilder()
                .WithAgreementDocuments(AGREEMENTDOCUMENTS)
                .WithUser(new UserEntity()
                {
                    IdentUserId = USER_ID
                })
                .Build();

            _svc = CreateService();

            await _svc.UserAgreement();

            _db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        private AgreementDocumentService CreateService()
        {
            return new AgreementDocumentService(_db.Object, _mapper, _userContext.Object, _securityService.Object);
        }
    }
}
