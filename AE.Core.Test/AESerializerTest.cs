using AE.Core.Serializer;

namespace AE.Core.Test;

public class AESerializerTest
{
	private static AESerializer serializer;

	[SetUp]
	public void Setup()
	{
		serializer = new AESerializer();
	}

	public enum TestEnum
	{
		V1,
		V2,
		V3,
	}

	[AESerializable]
	public class TestSerializer
	{
		public bool V { get; set; }
		public int V1 { get; set; }
		[AEIgnore]
		public int V1Ignore { get; set; }
		public float V2 { get; set; }
		public double V3 { get; set; }
		public string Text { get; set; }
		public DateTime Date { get; set; }
		public TimeSpan Time { get; set; }
		public DateTime? DateNull { get; set; }
		public Guid Guid { get; set; }
		public TestEnum Enum { get; set; }
		public int[] Ints { get; set; }
		public List<float> Floats { get; set; }
		public SubTestSerializer SubTest { get; set; }
	}

	[AESerializable]
	public class SubTestSerializer
	{
		public string Text { get; set; }
	}

	[Test]
	public void TestTypes()
	{
		var obj = new TestSerializer
		{
			V = true,
			V1 = 1,
			V1Ignore = 1,
			V2 = 2.2f,
			V3 = 3.3,
			Text = "~[Test&^$(']",
			Date = new DateTime(2000, 1, 1),
			Time = new TimeSpan(1, 2, 3, 4, 5) - new TimeSpan(2, 3, 4, 5, 6),
			DateNull = null,
			Guid = Guid.NewGuid(),
			Enum = TestEnum.V3,
			Ints = [1, 2],
			Floats = [2.2f, 3.3f],
			SubTest = new SubTestSerializer
			{
				Text = "~[Test&^$(']",
			},
		};

		var data = serializer.Serialize(obj);
		var obj2 = serializer.Deserialize<TestSerializer>(data);

		if (obj2.V == false)
			Assert.Fail();

		if (obj2.V1 != 1)
			Assert.Fail();

		if (obj2.V1Ignore == 1)
			Assert.Fail();

		if (obj2.V2 != 2.2f)
			Assert.Fail();

		if (obj2.V3 != 3.3)
			Assert.Fail();

		if (obj2.Text != "~[Test&^$(']")
			Assert.Fail();

		if (obj2.Date != new DateTime(2000, 1, 1))
			Assert.Fail();

		if (obj2.Time != new TimeSpan(1, 2, 3, 4, 5) - new TimeSpan(2, 3, 4, 5, 6))
			Assert.Fail();

		if (obj2.DateNull != null)
			Assert.Fail();

		if (obj2.Enum != TestEnum.V3)
			Assert.Fail();

		if (obj2.Ints[0] != 1 || obj2.Ints[1] != 2)
			Assert.Fail();

		if (obj2.Floats[0] != 2.2f || obj2.Floats[1] != 3.3f)
			Assert.Fail();

		if (obj2.SubTest == null || obj2.SubTest.Text != "~[Test&^$(']")
			Assert.Fail();
	}

	[TearDown]
	public void Dispose()
	{
		serializer?.Dispose();
	}
}