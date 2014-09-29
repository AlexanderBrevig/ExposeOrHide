using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExposeOrHide
{
    public static class Exposer
    {
        /// <summary>
        /// Dictate which members should be accessible
        /// </summary>
        /// <typeparam name="T">Type we want to Expose members on</typeparam>
        /// <param name="instance">The instance we want to work with</param>
        /// <param name="whiteList">The propery selector, white list</param>
        /// <returns>An Exposed object representing obj with only white list properties available</returns>
        public static Exposed<T> Expose<T>(this T instance, Expression<Func<T, dynamic>> whiteList) where T : class
        {
            var whiteListedProperties = PropertyHelper.GetProperties<T>(whiteList);
            var dictionary = whiteListedProperties.ToList().ToDict(instance);

            return new Exposed<T>(instance, dictionary);
        }

        /// <summary>
        /// Dictate which members should not be accessible
        /// </summary>
        /// <typeparam name="T">Type we want to Hide members on</typeparam>
        /// <param name="instance">The instance we want to work with</param>
        /// <param name="blackList">The propery selector, black list</param>
        /// <returns>An Exposed object representing obj with only none of the black list properties available</returns>
        public static Exposed<T> Hide<T>(this T instance, Expression<Func<T, dynamic>> blackList) where T : class
        {
            var blackListedProperties = PropertyHelper.GetProperties<T>(blackList).ToList();
            var all = typeof(T).GetProperties().ToList();
            var props = all.Except(blackListedProperties).ToList();
            var x = props.ToDict(instance);

            return new Exposed<T>(instance, x);
        }

        /// <summary>
        /// Convert a list of PropertyInfo to a dictionary of PropertyInfo.Name and PropertyInfo.GetValue(obj)
        /// </summary>
        /// <param name="props">The list of properties</param>
        /// <param name="obj">The object that owns these properties</param>
        /// <returns>A dictionary of name and value pairs for each property for obj</returns>
        public static Dictionary<string, dynamic> ToDict(this List<PropertyInfo> props, dynamic obj)
        {
            var x = new Dictionary<string, dynamic>();
            props.ToList().ForEach(pr => {
                x.Add(pr.Name, pr.GetValue(obj));
            });
            return x;
        }

        /// <summary>
        /// A class for maintaining the ExposeOrHide state for each property on an instance of T
        /// </summary>
        /// <typeparam name="T">The original Type we're working on</typeparam>
        public class Exposed<T> : Dictionary<string, dynamic> where T : class
        {
            internal Exposed(T instance, Dictionary<string, dynamic> data)
            {
                data.ToList().ForEach(kvp => {
                    this.Add(kvp.Key, kvp.Value);
                });
                this.instance = instance;
            }

            /// <summary>
            /// Expose rules for a member of this Exposed object
            /// </summary>
            /// <typeparam name="U">The type of the member we want to apply rules for</typeparam>
            /// <param name="selector">Select the member on the instance that exposes this</param>
            /// <param name="func">Express which Properties we want to expose</param>
            /// <returns>The modified instance of this Exposed object, with the new rules applied</returns>
            public Exposed<T> ExposeMember<U>(Expression<Func<T, dynamic>> selector, Expression<Func<U, dynamic>> func) where U : class
            {
                var pro = PropertyHelper.GetProperties<T>(selector);
                var first = pro.First();

                this[first.Name] = new List<dynamic>();
                if (first.GetValue(instance) is ICollection<U>) {
                    (first.GetValue(instance) as List<U>).ForEach(o => {
                        this[first.Name].Add(o.Expose<U>(func));
                    });
                } else if (first.GetValue(instance) is U) {
                    this[first.Name] = (first.GetValue(instance) as U).Expose<U>(func);
                }

                return this;
            }

            /// <summary>
            /// Hide rules for a member of this Exposed object
            /// </summary>
            /// <typeparam name="U">The type of the member we want to apply rules for</typeparam>
            /// <param name="selector">Select the member on the instance that hides this</param>
            /// <param name="func">Express which Properties we want to hide</param>
            /// <returns>The modified instance of this Exposed object, with the new rules applied</returns>
            public Exposed<T> HideMember<U>(Expression<Func<T, dynamic>> selector, Expression<Func<U, dynamic>> func) where U : class
            {
                var pro = PropertyHelper.GetProperties<T>(selector);
                var first = pro.First();

                if (first.GetValue(instance) is ICollection<U>) {
                    this[first.Name] = new List<dynamic>();
                    (first.GetValue(instance) as List<U>).ForEach(o => {
                        this[first.Name].Add(o.Hide<U>(func));
                    });
                } else if (first.GetValue(instance) is U) {
                    this[first.Name] = (first.GetValue(instance) as U).Hide<U>(func);
                }

                return this;
            }

            public dynamic ToDynamic()
            {
                return ToDynamic(this);
            }

            private dynamic ToDynamic(ICollection<KeyValuePair<string, object>> coll)
            {
                var expando = new ExpandoObject();
                var expandoCollection = (ICollection<KeyValuePair<string, object>>)expando;

                foreach (var kvp in coll) {
                    if (kvp.Value is IEnumerable<object>) {
                        var list = new List<dynamic>();
                        foreach(var obj in (kvp.Value as IEnumerable)) {
                            var dynamicChild = ToDynamic(obj as ICollection<KeyValuePair<string, object>>);
                            list.Add(dynamicChild);
                        }

                        expandoCollection.Add(new KeyValuePair<string, object>(kvp.Key, list));
                    } else {
                        expandoCollection.Add(kvp);
                    }
                }

                dynamic dynamicReturn = expando;

                return dynamicReturn;
            }



            private readonly T instance;
        }

    }
}
