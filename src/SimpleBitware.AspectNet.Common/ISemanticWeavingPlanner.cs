using Microsoft.CodeAnalysis;

namespace SimpleBitware.AspectNet.Common;

public interface ISemanticWeavingPlanner
{
    SemanticWeavingPlan? TryGenerateWeavingPlan(in WeaveCandidate candidate);
}
