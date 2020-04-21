using System.Runtime.Serialization;

namespace Loadshop.DomainServices.Validation.Data.Address.Enums
{
    public enum GeometryLocationType
    {
        [EnumMember(Value = "ROOFTOP")]
        Rooftop,
        [EnumMember(Value = "RANGE_INTERPOLATED")]
        Range_Interpolated,
        [EnumMember(Value = "GEOMETRIC_CENTER")]
        Geometric_Center,
        [EnumMember(Value = "APPROXIMATE")]
        Approximate
    }
}
