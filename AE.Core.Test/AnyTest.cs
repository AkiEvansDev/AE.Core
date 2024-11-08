using AE.Dal;

namespace AE.Core.Test;

public class AnyTest
{
    [Test]
    public void Test()
    {
        var result = Keys.A.GetDescriptions<Keys>().ToList();

        var c = ColorPath.GetColor("#880088", FactorType.Tint, 2);
    }
}