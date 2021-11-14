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
        public static T Cast<T>(JToken? token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            T obj = token.ToObject<T>();
            if (obj == null) throw new JsonParsingException($"Cannot parse JSON as object of type {typeof(T)} {token.ToString()}");
            return obj;
        }
    }
}