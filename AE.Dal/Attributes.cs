using System;

namespace AE.Dal
{
	/// <summary>
	/// Marks class as serializable
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class AESerializableAttribute : Attribute { }

	/// <summary>
	/// Marks property as ignore serializable
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class AEIgnoreAttribute : Attribute { }
}
