using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Loadshop.Web.API.Models
{
    public class PageableResponse<T>
    {
        public List<T> Data { get; }
        public int TotalRecords { get; }

        public PageableResponse(List<T> data, int totalRecords)
        {
            Data = data;
            TotalRecords = totalRecords;
        }
    }
}
