using Newtonsoft.Json;
using System;
using System.Linq;

namespace Loadshop.Web.API.Models.Shipping
{
    public class OrderEntryLoadContactViewModel
    {
        public Guid? LoadContactId { get; set; }
        public string Display { get; set; }
        public string Email { get; set; }
        [JsonProperty("phoneNumber")]
        public string Phone { get; set; }

        public string FirstName
        {
            get
            {
                var result = string.Empty;
                if (!string.IsNullOrWhiteSpace(Display))
                {
                    result = Display.Split(' ').FirstOrDefault();
                }

                return result;
            }
        }
        public string LastName
        {
            get
            {
                var result = string.Empty;
                if (!string.IsNullOrWhiteSpace(Display))
                {
                    var items = Display.Split(' ').ToList();
                    items.RemoveAt(0);
                    result = string.Join(" ", items);
                }

                return result;
            }
        }
    }
}
