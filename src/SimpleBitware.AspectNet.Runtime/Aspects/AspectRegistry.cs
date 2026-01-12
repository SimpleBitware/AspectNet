using System;
using System.Collections.Generic;

namespace SimpleBitware.AspectNet.Runtime.Aspects;

public sealed class AspectRegistry : IAspectRegistry
{
    private readonly Dictionary<string, AspectDescriptor[]> _map = new(StringComparer.Ordinal);

    public void Register(string methodId, params AspectDescriptor[] descriptors)
        => _map[methodId] = descriptors;

    public IReadOnlyList<AspectDescriptor> GetDescriptors(string methodId)
        => _map.TryGetValue(methodId, out var d) ? d : Array.Empty<AspectDescriptor>();
}