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
using DocumentService.SDK.Version.V1;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Http;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public class LoadshopDocumentServiceTests : IClassFixture<TestFixture>
    {
        private readonly IMapper _mapper;
        private readonly Mock<IUserContext> _userContext;
        private readonly Mock<IDocumentApiClient> _documentApiClient;
        private Mock<LoadshopDataContext> _db;
        private Mock<ISecurityService> _securityService;
        private LoadshopDocumentService _svc;
        private static Guid LOAD_DOCUMENT_ID = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static Guid LOAD_ID = Guid.Parse("11111111-1111-1111-1111-111111111122");

        private List<LoadDocumentEntity> LOADDOCUMENTS = new List<LoadDocumentEntity>()
            {
                new LoadDocumentEntity()
                {
                    LoadDocumentId = LOAD_DOCUMENT_ID,
                    FileName="test.jpg",
                    LoadId = LOAD_ID,
                },
         };

        public LoadshopDocumentServiceTests(TestFixture fixture)
        {
            _mapper = fixture.Mapper;
            _userContext = new Mock<IUserContext>();
            _documentApiClient = new Mock<IDocumentApiClient>();
            _db = new MockDbBuilder()
                .WithLoadDocuments(LOADDOCUMENTS)
                .WithLoad(new LoadEntity()
                {
                    LoadId = LOAD_ID
                })
                .Build();

            _securityService = new Mock<ISecurityService>();
            _securityService.Setup(x => x.UserHasAction(It.IsAny<string[]>())).Returns(true);
        }

        [Fact]
        public void GetDocumentTypes_Terms_ShouldReturnExpectedAgreement()
        {
            _svc = CreateService();

            var documentTypeDatas = _svc.GetDocumentTypes();

            documentTypeDatas.Count().Should().Be(2);

            documentTypeDatas.Single(x => x.Description == "Proof of Delivery").Should().NotBeNull();
            documentTypeDatas.Single(x => x.Description == "Other").Should().NotBeNull();
        }

        [Fact]
        public void GetDocument_Throws_WhenInvalidDocumentId()
        {
            _svc = CreateService();

            _svc.Awaiting(x => x.GetDocument(Guid.Empty)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Document not found");

            _documentApiClient.Verify(x => x.GetDocumentContent(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetDocument_Returns_WhenValidDocumentId()
        {
            _documentApiClient.Setup(x => x.GetDocumentContent(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new TMS.Infrastructure.Messaging.Client.FileMemoryStream()
            {
                FileName = "test.jpg"
            });

            _svc = CreateService();

            var result = await _svc.GetDocument(LOAD_DOCUMENT_ID);

            result.Should().NotBeNull();
            result.FileName.Should().Be("test.jpg");
            _documentApiClient.Verify(x => x.GetDocumentContent(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void RemoveDocument_Throws_WhenInvalidDocumentId()
        {
            _svc = CreateService();

            _svc.Awaiting(x => x.RemoveDocument(Guid.Empty)).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Document not found");

            _documentApiClient.Verify(x => x.DeleteDocument(It.IsAny<int>()), Times.Never);
            _db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RemoveDocument_Returns_WhenValidDocumentId()
        {
            _svc = CreateService();

            await _svc.RemoveDocument(LOAD_DOCUMENT_ID);

            _documentApiClient.Verify(x => x.DeleteDocument(It.IsAny<int>()), Times.Once);
            _db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void UploadDocument_Throws_WhenInvalidDocumentType()
        {
            _svc = CreateService();

            _svc.Awaiting(x => x.UploadDocument(new LoadDocumentUploadData()
            {
                LoadDocumentType = null
            })).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Invalid load document type");

            _documentApiClient.Verify(x => x.DeleteDocument(It.IsAny<int>()), Times.Never);
            _db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void UploadDocument_Throws_WhenInvalidDocumentTypeId()
        {
            _svc = CreateService();

            _svc.Awaiting(x => x.UploadDocument(new LoadDocumentUploadData()
            {
                LoadDocumentType = new LoadDocumentTypeData()
                {
                    Id = 0
                }
            })).Should()
                    .Throw<ValidationException>()
                    .WithMessage("Invalid load document type");

            _documentApiClient.Verify(x => x.DeleteDocument(It.IsAny<int>()), Times.Never);
            _db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UploadDocument_UploadsToDocumentService()
        {
            var fileMock = new Mock<IFormFile>();

            _documentApiClient.Setup(x => x.CreateDocument(It.IsAny<DocumentService.SDK.Version.V1.Model.DocumentCreate>()))
                .ReturnsAsync(new TMS.Infrastructure.Messaging.Common.MessageResult<DocumentService.Domain.Data.DocumentResultRowData>("", "Processed")
                {
                    ResultData = new DocumentService.Domain.Data.DocumentResultRowData()
                    {
                        DocContentId = 2,
                        DocHeaderId = 1
                    }
                });

            _svc = CreateService();

            var result = await _svc.UploadDocument(new LoadDocumentUploadData()
            {
                LoadDocumentType = new LoadDocumentTypeData()
                {
                    Id = (int)LoadshopDocumentServiceDocumentTypes.ProofOfDelivery
                },
                LoadId = LOAD_ID,
                File = fileMock.Object
            });


            result.Should().NotBeNull();

            _documentApiClient.Verify(x => x.CreateDocument(It.IsAny<DocumentService.SDK.Version.V1.Model.DocumentCreate>()), Times.Once);
            _db.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        private LoadshopDocumentService CreateService()
        {
            return new LoadshopDocumentService(_db.Object, _documentApiClient.Object, _userContext.Object, new Mock<ILogger<LoadshopDocumentService>>().Object, _mapper, _securityService.Object);
        }
    }
}
