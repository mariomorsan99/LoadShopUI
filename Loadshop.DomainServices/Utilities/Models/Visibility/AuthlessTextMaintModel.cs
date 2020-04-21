namespace Loadshop.DomainServices.Utilities.Models.Visibility
{
    public class AuthlessTextMaintModel
    {
        public string PhoneNumber { get; set; }
        public string LoadNbr { get; set; }

        public class Item1Data
        {
            public string Id { get; set; }
            public string Nbr { get; set; }
            public string Spn { get; set; }
            public string Sid { get; set; }
            public string Ca { get; set; }
            public string On { get; set; }
            public string Ol { get; set; }
            public string Dn { get; set; }
            public string Dl { get; set; }
            public string Fs { get; set; }
            public string Pro { get; set; }
            public string Ss { get; set; }
            public string Oo { get; set; }
            public string Co { get; set; }
            public string Ls { get; set; }
            public string Lsd { get; set; }
            public int M { get; set; }
            public bool Lic { get; set; }
            public long Dt { get; set; }
            public string Lsc { get; set; }
            public string Lsl { get; set; }
            public string Lsdt { get; set; }
            public int Tm { get; set; }
            public string Lsci { get; set; }
        }

        public class Item2Data
        {
            public string Carrier { get; set; }
            public string City { get; set; }
            public string CleansedPhoneNumberE164 { get; set; }
            public string CleansedPhoneNumberNational { get; set; }
            public string Country { get; set; }
            public string CountryCodelso2 { get; set; }
            public string CountryCodeNumeric { get; set; }
            public string County { get; set; }
            public string OriginalCountryCodelso2 { get; set; }
            public string OriginalPhoneNumber { get; set; }
            public string PhoneType { get; set; }
            public int PhoneTypeCode { get; set; }
            public string Timezone { get; set; }
            public string ZipCode { get; set; }
        }

        public class Data
        {
            public Item1Data Item1 { get; set; }
            public Item2Data Item2 { get; set; }
        }
    }
}
