using System.ComponentModel;

namespace ETLCommon
{
    /// <summary>
    /// Результат выполнения этапа
    /// </summary>
    public enum StageStatus
    {
        [Description("Успешно")]
        Successful,

        [Description("С предупреждениями")]
        Warnings,

        [Description("С ошибками")]
        Errors
    }
}
