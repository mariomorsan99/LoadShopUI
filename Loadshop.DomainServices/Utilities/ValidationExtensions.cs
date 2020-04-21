using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Loadshop.DomainServices.Utilities
{
    public static class ValidationExtensions
    {
        public static void NullEntityCheck<T>(this T entity, object id) where T : class
        {
            if (entity == null)
                throw new Exception($"{typeof(T).Name} could not be found for id: {id}");
        }

        public static void NullArgumentCheck<T>(this T arg, string argName = null)
        {
            var exception = new ArgumentNullException($"{argName ?? nameof(arg)} cannot be null");
            if (typeof(T) == typeof(string) && string.IsNullOrWhiteSpace(arg as string))
            {
                throw exception;
            }
            else if (typeof(T) == typeof(Guid) && default(Guid).Equals(arg))
            {
                throw exception;
            }
            else if (arg == null)
            {
                throw exception;
            }
        }

        public static TResult NullPropagate<T, TResult>(this T inObject, Expression<Func<T, TResult>> propertyExpression)
        {
            if (inObject == null)
                return default;

            var memberExpressions = BuildMemberExpressions(propertyExpression.Body as MemberExpression);
            var currentType = typeof(T);
            object currentObject = inObject;

            while (memberExpressions.Count > 0)
            {
                var expression = memberExpressions.Dequeue();
                var propertyName = expression.Member.Name;

                var value = currentType.GetProperty(propertyName)?.GetValue(currentObject);

                if (value == null)
                    return default;

                currentType = expression.Type;
                currentObject = value;
            }

            return (TResult)currentObject;
        }

        private static Queue<MemberExpression> BuildMemberExpressions(MemberExpression memberExpression)
        {
            var expressions = new Queue<MemberExpression>();

            if (memberExpression.Expression is MemberExpression)
            {
                var subExpressions = BuildMemberExpressions(memberExpression.Expression as MemberExpression);

                while (subExpressions.Count > 0)
                {
                    expressions.Enqueue(subExpressions.Dequeue());
                }
            }

            expressions.Enqueue(memberExpression);

            return expressions;
        }
    }
}
