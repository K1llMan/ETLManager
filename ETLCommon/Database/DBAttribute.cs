using System;

namespace ETLCommon
{
    /// <summary>
    /// Описание атрибута базы данных
    /// </summary>
    public class DBAttribute
    {
        /// <summary>
        /// Значение по умолчанию
        /// </summary>
        public object Default { get; internal set; }

        /// <summary>
        /// Имя атрибута
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Может ли принимать пустое значение
        /// </summary>
        public bool Nullable { get; internal set; }

        /// <summary>
        /// Размер поля
        /// </summary>
        public int Size { get; internal set; }

        /// <summary>
        /// Тип
        /// </summary>
        public Type Type { get; internal set; }
    }
}
