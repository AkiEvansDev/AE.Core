using AE.Core.Serializer;
using AE.Dal;

namespace AE.Core.Test;

public class AnyTest
{
	[Test]
	public void Test()
	{
		var result = Keys.A.GetDescriptions<Keys>().ToList();
    }
}