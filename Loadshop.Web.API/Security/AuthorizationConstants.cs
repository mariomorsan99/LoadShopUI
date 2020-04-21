namespace Loadshop.Web.API.Security
{
    public class AuthorizationConstants
    {
        public const string RolePolicyPrefix = "HasRole-";
        public const string ActionPolicyPrefix = "HasAction-";
        public const string HasAnyLoadShopRolePolicy = nameof(HasAnyLoadShopRolePolicy);
        public const string IsCarrierPolicy = nameof(IsCarrierPolicy);
        public const string IsShipperPolicy = nameof(IsShipperPolicy);
        public const string IsCarrierOrShipperPolicy = nameof(IsCarrierOrShipperPolicy);
        public const string IsCarrierOrShipperAdmin = nameof(IsCarrierOrShipperAdmin);
        public const string CanAccessCarrierLoadsPolicy = nameof(CanAccessCarrierLoadsPolicy);
        public const string CanAccessLoadDetail = nameof(CanAccessLoadDetail);
        public const string CanAccessLoadStatus = nameof(CanAccessLoadStatus);
    }
}
