namespace SimpleBitware.AspectNet.Tests.Weaving.Attributes;

public record ActivityKey(Type Type, string MemberName, int NumberOfParameters = 0);
