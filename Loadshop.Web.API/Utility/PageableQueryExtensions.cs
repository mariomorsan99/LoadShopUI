using Loadshop.DomainServices.Common.Services.QueryWrappers;
using Loadshop.Web.API.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Loadshop.Web.API.Utility
{
    public static class PageableQueryExtensions
    {
        public static PageableQuery<T> HandlePaging<T>(this PageableQuery<T> query, HttpRequest request)
        {
            var take = TryParse(request.Query[PageableQueryConstants.TakeQuery]);
            var skip = TryParse(request.Query[PageableQueryConstants.SkipQuery]);

            if (skip.HasValue)
            {
                query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query.Take(take.Value);
            }

            return query;
        }

        public static PageableQuery<T> HandleSorting<T>(this PageableQuery<T> query, HttpRequest request)
        {
            var orderByQuery = request.Query[PageableQueryConstants.OrderByQuery];
            var descedning = Convert.ToBoolean(request.Query[PageableQueryConstants.DecendingQuery]);

            if (!string.IsNullOrWhiteSpace(orderByQuery))
            {
                var expression = BuildPropertyExpression<T>(orderByQuery);

                InvokeOrderBy(query, expression, descedning);
            }

            return query;
        }

        public static async Task<PageableResponse<T>> ToPageableResponse<T>(this PageableQuery<T> query)
        {
            return new PageableResponse<T>(query.ToList(), await query.GetTotalRecords());
        }

        static int? TryParse(string str)
        {
            int outInt;

            if (int.TryParse(str, out outInt))
                return outInt;

            return null;
        }

        static LambdaExpression BuildPropertyExpression<T>(string property)
        {
            string[] props = property.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (string prop in props)
            {
                PropertyInfo pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            //Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            return lambda;
        }

        static void InvokeOrderBy<T>(PageableQuery<T> query, LambdaExpression expression, bool descending)
        {
            var orderByMethodName = descending ? "OrderByDescending" : "OrderBy";

            var orderByMethod = typeof(PageableQuery<T>).GetMethod(orderByMethodName);

            var genericOrderByMethod = orderByMethod.MakeGenericMethod(expression.ReturnType);

            genericOrderByMethod.Invoke(query, new [] { expression });
        }
    }
}
