using System;
using Newtonsoft.Json.Linq;

namespace MSFSModManager.Core.Parsing
{


    public class JsonParsingException : Exception
    {
        public JsonParsingException(string message)
            : base(message)
        {

        }
    }

    public static class JsonUtils
    {
        public static T Cast<T>(JToken token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            T obj = token.ToObject<T>();
            if (obj == null) throw new JsonParsingException($"Cannot parse JSON as type {typeof(T)} {token.ToString()}");
            return obj;
        }

        public static T CastMember<T>(JToken token, string member)
        {
            if (!(token is JObject)) throw new JsonParsingException($"Provided JSON is not an object: {token}.");
            return CastMember<T>((JObject)token, member);
        }

        public static T CastMember<T>(JObject obj, string member)
        {
            if (!obj.ContainsKey(member)) throw new JsonParsingException($"JSON has no field '{member}'.");
            return Cast<T>(obj[member]!);
        }
    }
}