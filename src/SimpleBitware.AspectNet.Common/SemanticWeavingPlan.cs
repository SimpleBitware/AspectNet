using System.Collections.Generic;

namespace SimpleBitware.AspectNet.Common;

/// <summary>
/// The complete weaving plan for a single member.
/// Weavers consume this to generate language-specific code.
/// </summary>
public sealed record SemanticWeavingPlan(
    WeaveCandidate Candidate,
    IReadOnlyList<AspectInstance> Aspects);
