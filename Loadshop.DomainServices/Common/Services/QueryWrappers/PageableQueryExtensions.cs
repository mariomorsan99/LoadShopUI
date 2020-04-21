using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loadshop.DomainServices.Common.Services.QueryWrappers
{
    public static class PageableQueryExtensions
    {
        public static PageableQuery<T> ToPageableQuery<T>(this IQueryable<T> queryable, Func<List<T>, Task> _queryExecuteFuncAsync = null, int maxResult = 100)
        {
            return new PageableQuery<T>(queryable, _queryExecuteFuncAsync, maxResult);
        }
    }
}
