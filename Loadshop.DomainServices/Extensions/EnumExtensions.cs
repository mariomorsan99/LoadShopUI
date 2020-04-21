using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Loadshop.DomainServices.Extensions
{
    public static class EnumExtensions
    {
        public static string GetEnumDescription(this Enum value)
        {
            return value.GetType()
                    .GetMember(value.ToString())
                    .FirstOrDefault()
                    ?.GetCustomAttribute<DescriptionAttribute>()
                    ?.Description
                    ?? value.ToString();
        }
    }
}
