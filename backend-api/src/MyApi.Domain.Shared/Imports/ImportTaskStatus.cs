using System.ComponentModel;

namespace MyApi.Domain.Shared.Imports;

public enum ImportTaskStatus
{
    [Description("排队中")]
    Pending = 0,

    [Description("导入中")]
    Processing = 1,

    [Description("导入成功")]
    Succeeded = 2,

    [Description("导入失败")]
    Failed = 3,

    [Description("部分成功")]
    PartiallySucceeded = 4
}
