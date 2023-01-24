using System;

namespace AE.Core
{
    /// <summary>
    /// Marks class as serializable
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class AWSerializableAttribute : Attribute { }

    /// <summary>
    /// Marks class as ignore serializable
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AWIgnoreAttribute : Attribute { }

    /// <summary>
    /// Marks class as reference (need <see cref="IReference"/>)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AWReferenceAttribute : Attribute { }
}
