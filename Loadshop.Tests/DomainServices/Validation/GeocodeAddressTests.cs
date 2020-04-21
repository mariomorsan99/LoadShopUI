using FluentAssertions;
using Loadshop.DomainServices.Validation.Data;
using Loadshop.DomainServices.Validation.Data.Address;
using System.Collections.Generic;
using Loadshop.Tests.DomainServices.Loadshop;
using Xunit;
using Loadshop.DomainServices.Validation.Services;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using System.Threading;
using System.Net;
using Loadshop.DomainServices.Loadshop.Services.Data;
using Microsoft.Extensions.Configuration;

namespace Loadshop.Tests.DomainServices.Validation
{
    public class GeocodeAddressTests
    {
        [Fact]
        public void GetAddressComponent_Null()
        {
            GeocodeAddress.GetAddressComponent(null, null).Should().BeNull();
        }

        [Fact]
        public void GetAddressComponent_Empty()
        {
            GeocodeAddress.GetAddressComponent(new List<AddressComponent>(), new List<string>()).Should().BeNull();
        }

        [Fact]
        public void GetAddressComponent_NotFound()
        {
            var addressComponents = new List<AddressComponent>()
            {
                new AddressComponent()
                {
                    ShortName = "one",
                    Types = new List<string>()
                    {
                        "administrative_area_level_1",
                        "airport"
                    }
                },
                new AddressComponent()
                {
                    ShortName = "two",
                    Types = new List<string>()
                    {
                        "country",
                        "geocode"
                    }
                }
            };
            var types = new List<string>()
            {
                "floor"
            };

            GeocodeAddress.GetAddressComponent(addressComponents, types).Should().BeNull();

            types = new List<string>()
            {
                "country"
            };

            GeocodeAddress.GetAddressComponent(addressComponents, types).Should().BeNull();
        }

        [Fact]
        public void GetAddressComponent()
        {
            var addressComponents = new List<AddressComponent>()
            {
                new AddressComponent()
                {
                    ShortName = "city",
                    Types = new List<string>()
                    {
                        "locality",
                        "political"
                    }
                },
                new AddressComponent()
                {
                    ShortName = "state",
                    Types = new List<string>()
                    {
                        "administrative_area_level_1",
                        "political"
                    }
                }
            };
            var types = new List<string>()
            {
                "locality",
                "political"
            };

            var result = GeocodeAddress.GetAddressComponent(addressComponents, types);
            result.Should().NotBeNull();
            result.ShortName.Should().Be("city");

            types = new List<string>()
            {
                "political",
                "administrative_area_level_1"
            };

            result = GeocodeAddress.GetAddressComponent(addressComponents, types);
            result.Should().NotBeNull();
            result.ShortName.Should().Be("state");
        }
    }
}
