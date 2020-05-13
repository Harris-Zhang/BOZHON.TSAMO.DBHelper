using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Attributes
{
    public static class CustomAttributeHelper
    { 
        /// <summary>
        /// 获取CustomAttribute Value
        /// </summary>
        /// <typeparam name="T">Attribute的子类型</typeparam>
        /// <param name="sourceType">头部标有CustomAttribute类的类型</param>
        /// <param name="attributeValueAction">取Attribute具体哪个属性值的匿名函数</param>
        /// <param name="name">field name或property name</param>
        /// <returns>返回Attribute的值，没有则返回null</returns>
        public static string GetCustomAttributeValue<T>(this Type sourceType, Func<T, string> attributeValueAction,
            string name = null) where T : Attribute
        {
            return GetValue(sourceType, attributeValueAction, name);
        }


        private static string GetValue<T>(Type type,
            Func<T, string> attributeValueAction, string name)
        {
            object attribute = null;
            if (string.IsNullOrEmpty(name))
            {
                attribute =
                    type.GetCustomAttributes(typeof(T), false).FirstOrDefault();
            }
            else
            {
                var propertyInfo = type.GetProperty(name);
                if (propertyInfo != null)
                {
                    attribute =
                        propertyInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault();
                }

                var fieldInfo = type.GetField(name);
                if (fieldInfo != null)
                {
                    attribute = fieldInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault();
                }
            }

            return attribute == null ? null : attributeValueAction((T)attribute);
        }
 
    }
}
