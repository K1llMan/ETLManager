namespace ETLCommon
{
    public class DBTablePage
    {
        /// <summary>
        /// Общее количество записей
        /// </summary>
        public decimal Total;

        /// <summary>
        /// Номер страницы
        /// </summary>
        public int Page;

        /// <summary>
        /// Размер
        /// </summary>
        public int PageSize;

        /// <summary>
        /// Количество страниц
        /// </summary>
        public int PageCount;

        /// <summary>
        /// Поле для сортировки
        /// </summary>
        public string OrderBy;

        /// <summary>
        /// Направление сортировки
        /// </summary>
        public string OrderDir;

        /// <summary>
        /// Данные страницы
        /// </summary>
        public dynamic Rows;
    }
}
