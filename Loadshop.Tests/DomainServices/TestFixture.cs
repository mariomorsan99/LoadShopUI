using AutoMapper;
using System.Reflection;

namespace Loadshop.Tests.DomainServices
{
    public class TestFixture
    {
        public readonly IMapper Mapper;
        public TestFixture()
        {
            var dll = Assembly.LoadFrom("Loadshop.DomainServices.dll");
            var config = new MapperConfiguration(c =>
            {
                c.AddMaps(dll);
            });

            Mapper = config.CreateMapper();
        }
    }
}
