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
using AE.Core.Serializer.Common;

[AESerializable]
public class Test
{
    public DateTime Date { get; set; } = DateTime.Now;
    public double Number { get; set; } = 2.09;

    public List<int> List{ get; set; } = new List<int>
    {
        1, 2, 3, 4, 5
    };

    public int[] Array { get; set; } = new int[2] { 1, 2 };

    public List<string> StringList { get; set; } = new List<string>
    {
       "qq", "ww", "ee"
    };

    public Dictionary<string, string> Dictionary { get; set; } = new Dictionary<string, string>
    {
        { "q", "qq" }, { "w", "ww" }, { "e", "ee" }
    };
}

var test = new Test();
string data = null;

using (var serializer = new AESerializer())
{
    // Get save data
    data = serializer.Serialize(test);
}

test = null;

using (var serializer = new AESerializer())
{
    // Load from data
    test = serializer.Deserialize<Test>(data);
}
```

SerializerHelper can save data to file as string or byte array. Example:
```C#
using AE.Core.Serializer.Common;

var test = new Test();
string data = null;

//...

SerializerHelper.SaveText(data, "fileName"); // .SaveByte() to save string as byte array

data = null;
test = null;

data = SerializerHelper.LoadText("fileName"); // .LoadByte() to load string from byte array

//...
```

Serializer can save all reference as one object. Example:
```C#
using AE.Core.Serializer;
using AE.Core.Serializer.Common;

[AESerializable]
public class Reference : IReference
{
    // Don't use this property 
    public int ReferenceId { get; set; }

    // Any data
}

[AESerializable]
public class TestReference
{
    // Source
    public Reference F1 { get; set; } = new Reference();

    // Reference to source will be saved after save and load
    [AWReference]
    public List<Reference> References { get; set; } = new List<Reference>();

    public TestReference()
    {
        References.Add(F1);
        References.Add(F1);
        References.Add(F1);
        References.Add(F1);
    }
}
```
