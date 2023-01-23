using System;

namespace Dalamud.Utility.Signatures;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SignatureExAttribute : Attribute
{
    public bool EnableHook { get; init; } = true;
    public bool DisposeHook { get; init; } = true;
}