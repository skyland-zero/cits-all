namespace Cits.OperationLogs;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class SkipOperationLogAttribute : Attribute
{
}