using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Koubot.Tool.Extensions
{
    /// <summary>
    /// Extension method of <seealso cref="Type"/> type.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Judge whether the specific type is nullable value type or not.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullableValueType(this Type type) => type.GetTypeInfo().IsGenericType &&
                                                                  type.GetGenericTypeDefinition() == typeof(Nullable<>);
        /// <summary>
        /// Judge whether the specific type is derived from giving generic type or not.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericType">typeof(GenericType&lt;&gt;)</param>
        /// <returns></returns>
        public static bool IsDerivedFromGenericType(this Type type, Type genericType) =>
            type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == genericType;
        /// <summary>
        /// Get the underlying type T in such as type of IEnumerable&lt;T&gt; and T?
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetUnderlyingType(this Type type)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                var innerType = type.GetGenericArguments().FirstOrDefault();
                type = innerType ?? throw new Exception($"{type.FullName} not support {nameof(GetUnderlyingType)}");
            }
            if (type.IsNullableValueType())
            {
                type = Nullable.GetUnderlyingType(type);
            }
            return type;
        }
    }
}