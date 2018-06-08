using System.ComponentModel;

namespace ETLCommon
{
    /// <summary>
    /// Статусы выполнения этапа закачки
    /// </summary>
    public enum StageStatus
    {
        [Description("Успешно")]
        Successful,

        [Description("С предупреждениями")]
        Warnings,

        [Description("С ошибками")]
        Errors,

        [Description("Пропущен")]
        Skipped
    }
}
