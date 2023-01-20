using System;

namespace Dalamud.Utility.Signatures;

public enum EnableHook
{
    Auto,
    Manual
}

public enum DisposeHook
{
    Auto,
    Manual
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SignatureExAttribute : Attribute
{
    public EnableHook EnableHook { get; init; }
    public DisposeHook DisposeHook { get; init; }
}