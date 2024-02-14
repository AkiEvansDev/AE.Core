# AE.Core

Project containing data logger and serializer.

## Logger

Logger can save exception, message and method name. Example:
```C#
using AE.Core.Log;

public void LogTest()
{
    ILogger logger = new Logger("error", "exeption.log");
    logger.Log("test");
}
// in exeption.log file
// [ERROR]: [21.06 03:17:20] - LogTest() - test
```
ILogger contains two methods:
```C#
void Log(string message, string tag = null, string method = null, bool ignoreEvent = false);
void Log(Exception ex, string message = null, string tag = null, string method = null, bool ignoreEvent = false);
```

`ignoreEvent` is needed so as not call an event `LoggerHelper.OnLog`

## Serializer

Serializer can save and load any data. Example:
```C#
using AE.Core.Serializer;

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
string data = null;

using (var serializer = new AESerializer())
{
    // Get save data
    data = serializer.Serialize(obj);
}

obj = null;

using (var serializer = new AESerializer())
{
    // Load from data
    obj = serializer.Deserialize<TestSerializer>(data);
	// or get object
    object result = serializer.Deserialize(data);
}
```

SerializerHelper can save data to file as string or byte array. Example:
```C#
using AE.Core;

var test = new TestSerializer();
string data = null;

//...

SerializerHelper.SaveText(data, "fileName");

data = null;
test = null;

data = SerializerHelper.LoadText("fileName");

//...
```

Serializer can save all reference as one object. Example:
```C#
using AE.Core.Serializer;

[AESerializable]
public class ReferenceTestArray
{
	public string Data { get; set; }
	public Dictionary<string, ReferenceTestArrayItem> Items { get; set; }
}

[AESerializable]
public class ReferenceTestArrayItem
{
	public string Data { get; set; }
	public ReferenceTestArray Parent { get; set; }
}

var r = new ReferenceTestArray
{
	Data = "parent",
	Items = new Dictionary<string, ReferenceTestArrayItem>(),
};

var r1 = new ReferenceTestArrayItem
{
	Data = "r1",
	Parent = r,
};

var r2 = new ReferenceTestArrayItem
{
	Data = "r2",
	Parent = r,
};

r.Items.Add("1", r1);
r.Items.Add("2", r2);
r.Items.Add("3", r1);

using (var serializer = new AESerializer())
{
	var data = serializer.Serialize(r);
	// all references to objects are saved (result.Items["1"].Parent == result, etc.)
	var result = serializer.Deserialize<ReferenceTestArray>(data);
}
```
