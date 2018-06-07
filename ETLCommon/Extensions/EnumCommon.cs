using System;
using System.ComponentModel;

namespace ETLCommon
{
    public static class EnumCommon
    {
        /// <summary>
        /// Возвращает описание значение, если доступен атрибут
        /// </summary>
        public static string GetDescription(this Enum enumValue)
        {
            DescriptionAttribute attr = (DescriptionAttribute)Attribute.GetCustomAttribute(
                enumValue.GetType().GetField(Convert.ToString(enumValue)),
                typeof(DescriptionAttribute));
            return attr == null 
                ? string.Empty 
                : attr.Description;
        }
    }
}
