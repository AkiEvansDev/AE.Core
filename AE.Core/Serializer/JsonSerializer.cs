using System.Text.Json;

namespace AE.Core.Serializer
{
    public static class JsonSerializer
    {
        public static string Serialize<T>(T obj, JsonSerializerOptions options = null)
            where T : class
        {
            return System.Text.Json.JsonSerializer.Serialize(obj, typeof(T), options);
        }

        public static T Deserialize<T>(string json, JsonSerializerOptions options = null)
            where T : class
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, options);
        }
    }
}
