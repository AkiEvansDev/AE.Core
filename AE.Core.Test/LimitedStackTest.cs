namespace AE.Core.Test;

public class LimitedStackTest
{
	[Test]
	public void Test()
	{
		var stack = new LimitedStack<int>(10);

		for (int i = 0; i < 20; i++)
			stack.Push(i);

		if (stack.Peek() != 19 || stack.Count != 10)
			Assert.Fail();
	}
}
