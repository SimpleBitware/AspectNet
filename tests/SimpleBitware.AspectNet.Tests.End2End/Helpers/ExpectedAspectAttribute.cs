using SimpleBitware.AspectNet.Abstractions.Attributes;

namespace SimpleBitware.AspectNet.Tests.End2End.Helpers;

public record ExpectedAspectAttribute(Type Type, int Priority, AspectNetAttributeContext Context);
