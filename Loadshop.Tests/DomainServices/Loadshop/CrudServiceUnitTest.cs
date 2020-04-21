using Loadshop.DomainServices.Common.Services.Crud;
using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Moq;
using AutoMapper;
using Loadshop.DomainServices.Security;
using Loadshop.DomainServices.Loadshop.Services;
using Loadshop.DomainServices.Loadshop.Services.Interfaces;

namespace Loadshop.Tests.DomainServices.Loadshop
{
    public abstract class CrudServiceUnitTest<TData, TService> : IClassFixture<TestFixture>
        where TService : ICrudService<TData>
    {
        protected readonly IMapper _mapper;
        protected readonly Mock<IUserContext> _userContext;
        protected readonly Mock<ISecurityService> _securityService;

        protected TService CrudService;

        public CrudServiceUnitTest(TestFixture fixture)
        {
            _mapper = fixture.Mapper;
            _userContext = new Mock<IUserContext>();
            _securityService = new Mock<ISecurityService>();
        }

        /// <summary>
        /// Should Return a collection that is not empty
        /// </summary>
        /// <typeparam name="TCollectionData">Data Object to map CrudService Output to</typeparam>
        /// <returns></returns>
        protected async Task GetCollectionTestHelper<TCollectionData>()
        {
            var userDataResult = await CrudService.GetCollection<TCollectionData>();

            userDataResult.Data.Should().NotBeEmpty();
        }

        /// <summary>
        /// Should return a TData object that matches restulData
        /// </summary>
        /// <param name="resultData">Should Match returned data for provided keys</param>
        /// <param name="keys">Key(s) of object to fetch from CrudServic</param>
        /// <returns></returns>
        protected async Task GetByKeyTestHelper(TData resultData, params object[] keys)
        {
            var getResult = await CrudService.GetByKey(keys);

            getResult.Data.Should().BeEquivalentTo(resultData);
        }

        protected async Task CreateTestHelper(
                                        TData createData,
                                        TData resultData,
                                        Func<EquivalencyAssertionOptions<TData>, EquivalencyAssertionOptions<TData>> equivalentOptions = null)
        {
            var getResult = await CrudService.Create(createData);


            getResult.Data.Should().BeEquivalentTo(resultData, equivalentOptions ?? GetDefaultEquivalencyAssertionOptions());
        }

        /// <summary>
        /// Should return a TData object that matches restultData
        /// </summary>
        /// <param name="createData">Data to map to Entity</param>
        /// <param name="resultData">Data that should match the output of the CrudService update</param>
        /// <param name="keys">Key(s) of the entity to update</param>
        /// <returns></returns>
        protected async Task UpdateTestHelper(
                                        TData createData,
                                        TData resultData,
                                        params object[] keys)
        {
            await UpdateTestHelper(createData, resultData, null, keys);
        }

        /// <summary>
        /// Should return a TData object that matches restultData
        /// </summary>
        /// <param name="updateData">Data to map to Entity</param>
        /// <param name="resultData">Data that should match the output of the CrudService update</param>
        /// <param name="equivalentOptions">Config for matching output</param>
        /// <param name="keys">Key(s) of the endity to update</param>
        /// <returns></returns>
        protected async Task UpdateTestHelper (
                                        TData updateData,
                                        TData resultData,
                                        Func<EquivalencyAssertionOptions<TData>, EquivalencyAssertionOptions<TData>> equivalentOptions,
                                        params object[] keys)
        {
            var getResult = await CrudService.Update(updateData, true, keys);

            getResult.Data.Should().BeEquivalentTo(resultData, equivalentOptions ?? GetDefaultEquivalencyAssertionOptions());
        }

        /// <summary>
        /// Should not error out    
        /// </summary>
        /// <param name="keys">Key(s) of entity to delete</param>
        /// <returns></returns>
        protected async Task DeleteHelper(params object[] keys)
        {
            var result = await CrudService.Delete(keys);

            result.Status.Should().Be(CrudResultStatus.Successful);
        }

        public abstract Task GetCollectionTest();

        public abstract Task GetByKeyTest();

        public abstract Task CreateTest();

        public abstract Task UpdateTest();

        public abstract Task DeleteTest();

        private Func<EquivalencyAssertionOptions<TData>, EquivalencyAssertionOptions<TData>> GetDefaultEquivalencyAssertionOptions()
        {
            return opt => opt;
        }
    }
}
