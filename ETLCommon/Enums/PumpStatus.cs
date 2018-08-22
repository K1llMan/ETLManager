using System.ComponentModel;

namespace ETLCommon
{
    /// <summary>
    /// Результат выполнения этапа
    /// </summary>
    public enum PumpStatus
    {
        [Description("Завершена успешно")]
        Successful,

        [Description("Завершена с предупреждениями")]
        Warnings,

        [Description("Завершена с ошибками")]
        Errors,

        [Description("Завершена досрочно")]
        Terminated,

        [Description("Не запускалась")]
        None,
    }
}
