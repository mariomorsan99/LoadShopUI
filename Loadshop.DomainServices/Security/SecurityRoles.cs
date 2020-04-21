namespace Loadshop.DomainServices.Security
{
    public static class SecurityRoles
    {
        public static string[] CarrierRoles = { CarrierAdmin, CarrierUser, CarrierUserViewOnly, SystemAdmin, LSAdmin, DedicatedPlanner, KBX_Outbound_Dedicated, KBX_AR, KBXAudit };
        public static string[] ShipperRoles = { ShipperAdmin, ShipperUser, ShipperUserViewOnly, SystemAdmin, LSAdmin, KBXPlanner, KBX_Outbound_Dedicated };
        public static string[] DedicatedRoles = {DedicatedPlanner, KBX_Outbound_Dedicated};
        public static string[] AdminRoles = { SystemAdmin, LSAdmin };

        public const string SystemAdmin = "System Admin";
        public const string LSAdmin = "LS Admin";
        public const string KBXPlanner = "KBX Planner";
        public const string KBX_AR = "KBX A/R";
        public const string KBXAudit = "KBX Audit";
        public const string KBXRating = "KBX Rating";
        public const string KBX_Outbound_Dedicated = "KBX Outbound/Dedicated";
        public const string DedicatedPlanner = "Dedicated Planner";
        public const string ShipperAdmin = "Shipper Admin";
        public const string CarrierAdmin = "Carrier Admin";
        public const string ShipperUser = "Shipper User";
        public const string CarrierUser = "Carrier User";
        public const string ShipperUserViewOnly = "Shipper User View Only";
        public const string CarrierUserViewOnly = "Carrier User View Only";
    }
}
