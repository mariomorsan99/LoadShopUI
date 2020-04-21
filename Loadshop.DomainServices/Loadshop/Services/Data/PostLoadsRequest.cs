using System;
using System.Collections.Generic;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class PostLoadsRequest
    {
        public DateTime RequestTime { get; set; } = DateTime.Now;
        public string CurrentUsername { get; set; }
        public List<PostingLoad> Loads { get; set; }
    }
}
