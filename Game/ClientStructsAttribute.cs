using System;

namespace Dalamud.Utility.Signatures;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ClientStructsAttribute : Attribute
{
    public Type ClientStructsType { get; init; }
    public string MemberName { get; init; }
    public ClientStructsAttribute(Type type, string memberName)
    {
        ClientStructsType = type;
        MemberName = memberName;
    }
}