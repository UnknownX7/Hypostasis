using System;

namespace Hypostasis.Game;

[AttributeUsage(AttributeTargets.Struct)]
public class GameStructureAttribute : Attribute
{
    public string CtorSignature { get; init; }
    public GameStructureAttribute(string ctor) => CtorSignature = ctor;
}