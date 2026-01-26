using System;
using SimpleBitware.AspectNet;

namespace SampleConsumer;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
public sealed class LogAttribute : AspectNetAttribute {}
