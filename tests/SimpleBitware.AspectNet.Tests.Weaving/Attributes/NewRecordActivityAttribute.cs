namespace SimpleBitware.AspectNet.Tests.Weaving.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, Inherited = false, AllowMultiple = true)]
public class NewRecordActivityAttribute : RecordActivityAttribute
{
}
