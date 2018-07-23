using System.Collections.Generic;

namespace ETLCommon
{
    public class Migration
    {
        #region Поля

        public decimal Version;
        public List<Dictionary<string, List<string>>> Up;
        public List<Dictionary<string, List<string>>> Down;

        #endregion Поля
    }
}
