using System;

using System.Collections.Generic;
using System.Reflection;

namespace Koubot.Tool.General
{
    /// <summary>
    /// 反射工具类
    /// </summary>
    public static class ReflectionTool
    {
        /// <summary>
        /// 获取指定类对象中所有public的属性信息，返回Dict[key属性名，Value[属性类型，属性值]]
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="instance">类的实例</param>
        /// <param name="toLowerCase">返回的属性名是否转小写</param>
        /// <returns></returns>
        public static Dictionary<string, KeyValuePair<Type, object>> GetAllPropertyInfo<T>(T instance, bool toLowerCase = false) where T : class
        {
            if (instance == null)
                return null;
            Dictionary<string, KeyValuePair<Type, object>> propertyInfoDict = new Dictionary<string, KeyValuePair<Type, object>>();
            Type type = typeof(T);
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                //获取属性类型
                Type propertyType = propertyInfo.PropertyType;
                //获取属性名：
                string propertyName = toLowerCase ? propertyInfo.Name.ToLower() : propertyInfo.Name;
                //获取属性值：
                object propertyValue = propertyInfo.GetValue(instance);
                KeyValuePair<Type, object> propertyPair = new KeyValuePair<Type, object>(propertyType, propertyValue);
                propertyInfoDict.Add(propertyName, propertyPair);
            }
            return propertyInfoDict;
        }

        /// <summary>
        /// 利用反射快速将model与数据库取出的数据赋值（需要model名字与数据库字段一致）
        /// </summary>
        /// <param name="instance">要赋值的model实例</param>
        /// <param name="propertyName">要赋值的model属性</param>
        /// <param name="value">要赋值的model值</param>
        public static void SetModelPropertyValue(object instance, string propertyName, object value)
        {
            instance.GetType().GetProperty(propertyName)?.SetValue(instance, value);
        }
    }
}
