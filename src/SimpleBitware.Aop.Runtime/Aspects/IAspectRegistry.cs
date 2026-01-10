using System.Collections.Generic;

namespace SimpleBitware.Aop.Runtime.Aspects;

public interface IAspectRegistry
{
    void Register(string methodId, params AspectDescriptor[] descriptors);
    IReadOnlyList<AspectDescriptor> GetDescriptors(string methodId);
}