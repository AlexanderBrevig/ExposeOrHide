using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ExposeOrHide
{
    public static class PropertyHelper
    {
        /// <summary>
        /// Get properties based on a LINQ like statement (model=>new {model.Property1, model.Property2})
        /// </summary>
        /// <typeparam name="T">The implicit type we're retrieving properties from</typeparam>
        /// <param name="propertyExpression">The actual expression that gets properties (one or many)</param>
        /// <returns>List of PropertyInfo matching the propertyExpression</returns>
        public static IEnumerable<PropertyInfo> GetProperties<T>(Expression<Func<T, dynamic>> propertyExpression)
        {
            return GetProperties(propertyExpression.Body);
        }

        private static IEnumerable<PropertyInfo> GetProperties(Expression expression)
        {
            if (expression == null) yield break;
            var wrp = expression as System.Linq.Expressions.NewExpression;
            if (wrp != null) {
                foreach (var arg in wrp.Arguments) {
                    foreach (var propertyInfo in GetProperties(arg)) {
                        yield return propertyInfo;
                    }
                }
            } else {
                var memberExpression = expression as MemberExpression;
                if (memberExpression == null) yield break;

                var property = memberExpression.Member as PropertyInfo;
                if (property == null) {
                    throw new ArgumentException("Expression is not a valid property accessor");
                }
                foreach (var propertyInfo in GetProperties(memberExpression.Expression)) {
                    yield return propertyInfo;
                }
                yield return property;
            }
        }
    }
}
