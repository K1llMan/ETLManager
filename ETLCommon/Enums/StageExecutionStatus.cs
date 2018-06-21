using System.ComponentModel;

namespace ETLCommon
{
    /// <summary>
    /// Статусы выполнения этапа закачки
    /// </summary>
    public enum StageExecutionStatus
    {
        [Description("В очереди")]
        InQueue,

        [Description("Выполняется")]
        Execution,

        [Description("Завершён")]
        Finished,

        [Description("Пропущен")]
        Skipped
    }
}
