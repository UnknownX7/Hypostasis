using System;

namespace Dalamud.Utility.Signatures;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ClientStructsAttribute : Attribute
{
    public Type ClientStructsType { get; init; }
    public string MemberName { get; init; } = "Instance";
    public ClientStructsAttribute(Type type) => ClientStructsType = type;
    public ClientStructsAttribute() { }
}

public class ClientStructsAttribute<T> : ClientStructsAttribute
{
    public ClientStructsAttribute() => ClientStructsType = typeof(T);
}