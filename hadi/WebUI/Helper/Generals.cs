using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebUI.Helper
{
    public static class Generals
    {
        #region Json
        public static string ToJson<T>(this T instance)
    => JsonConvert.SerializeObject(instance, JsonSettings.SerializerDefaults);
        public static T To<T>(this string json)
    => JsonConvert.DeserializeObject<T>(json, JsonSettings.SerializerDefaults);
        public static class JsonSettings
        {
            static readonly DefaultContractResolver _contractResolver =
                new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };

            internal static JsonSerializerSettings SerializerDefaults { get; } =
                new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    ContractResolver = _contractResolver,
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore
                };
        }

        //used
        //var json = address.ToJson();
        //var address = json.To<Address>();
        #endregion
    }
}
