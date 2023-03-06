// See https://aka.ms/new-console-template for more information
using AE.Core;
using AE.Core.Test;

Console.WriteLine("Hello, World!");




var t = new Test
{
    DateTime = DateTime.Now
};

var r = t.Serialize();
var t2 = r.Deserialize<Test>();

return;

