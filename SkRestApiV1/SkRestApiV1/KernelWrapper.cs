using Microsoft.SemanticKernel;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

public class KernelWrapper
{
    public Kernel Kernel { get; init; }
    public string Name { get; init; }

    public ImmutableList<string> ServiceIds { get; init; } = [] ;
}