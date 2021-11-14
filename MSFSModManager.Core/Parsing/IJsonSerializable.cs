using Newtonsoft.Json.Linq;

namespace MSFSModManager.Core
{
    public interface IJsonSerializable
    {
        JToken Serialize();
    }
}