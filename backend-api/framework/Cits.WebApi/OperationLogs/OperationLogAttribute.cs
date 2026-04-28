namespace Cits.OperationLogs;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class OperationLogAttribute : Attribute
{
    public OperationLogAttribute()
    {
    }

    public OperationLogAttribute(string module)
    {
        Module = module;
    }

    public OperationLogAttribute(string module, string operationType)
    {
        Module = module;
        OperationType = operationType;
    }

    public string? Module { get; set; }

    public string? OperationType { get; set; }
}