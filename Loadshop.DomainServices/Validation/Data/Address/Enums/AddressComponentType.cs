using System.Runtime.Serialization;

namespace Loadshop.DomainServices.Validation.Data.Address.Enums
{
    public enum AddressComponentType
    {
        [EnumMember(Value = "unknown")]
        Uknown,
        [EnumMember(Value = "street_address")]
        Street_Address,
        [EnumMember(Value = "route")]
        Route,
        [EnumMember(Value = "intersection")]
        Intersection,
        [EnumMember(Value = "political")]
        Political,
        [EnumMember(Value = "country")]
        Country,
        [EnumMember(Value = "administrative_area_level_1")]
        Administrative_Area_Level_1,
        [EnumMember(Value = "administrative_area_level_2")]
        Administrative_Area_Level_2,
        [EnumMember(Value = "administrative_area_level_3")]
        Administrative_Area_Level_3,
        [EnumMember(Value = "administrative_area_level_4")]
        Administrative_Area_Level_4,
        [EnumMember(Value = "administrative_area_level_5")]
        Administrative_Area_Level_5,
        [EnumMember(Value = "colloquial_area")]
        Colloquial_Area,
        [EnumMember(Value = "locality")]
        Locality,
        [EnumMember(Value = "ward")]
        Ward,
        [EnumMember(Value = "sublocality")]
        Sublocality,
        [EnumMember(Value = "sublocality_level_1")]
        Sublocality_Level_1,
        [EnumMember(Value = "sublocality_level_2")]
        SublocalityLevel2,
        [EnumMember(Value = "sublocality_level_3")]
        Sublocality_Level_3,
        [EnumMember(Value = "sublocality_level_4")]
        Sublocality_Level_4,
        [EnumMember(Value = "sublocality_level_5")]
        Sublocality_Level_5,
        [EnumMember(Value = "neighborhood")]
        Neighborhood,
        [EnumMember(Value = "premise")]
        Premise,
        [EnumMember(Value = "subpremise")]
        Subpremise,
        [EnumMember(Value = "postal_code")]
        Postal_Code,
        [EnumMember(Value = "natural_feature")]
        Natural_Feature,
        [EnumMember(Value = "airport")]
        Airport,
        [EnumMember(Value = "park")]
        Park,
        [EnumMember(Value = "floor")]
        Floor,
        [EnumMember(Value = "establishment")]
        Establishment,
        [EnumMember(Value = "point_of_interest")]
        Point_Of_Interest,
        [EnumMember(Value = "parking")]
        Parking,
        [EnumMember(Value = "post_box")]
        Post_Box,
        [EnumMember(Value = "postal_town")]
        Postal_Town,
        [EnumMember(Value = "room")]
        Room,
        [EnumMember(Value = "street_number")]
        Street_Number,
        [EnumMember(Value = "bus_station")]
        Bus_Station,
        [EnumMember(Value = "train_station")]
        Train_Station,
        [EnumMember(Value = "transit_station")]
        Transit_Station,
        [EnumMember(Value = "geocode")]
        Geocode,
        [EnumMember(Value = "postal_code_prefix")]
        Postal_Code_Prefix,
        [EnumMember(Value = "postal_code_suffix")]
        Postal_Code_Suffix,
        [EnumMember(Value = "tourist_attraction")]
        Tourist_Attraction,
        [EnumMember(Value = "travel_agency")]
        Travel_Agency,
        [EnumMember(Value = "campground")]
        Campground,
    }
}