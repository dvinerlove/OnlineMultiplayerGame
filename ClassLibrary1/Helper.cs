using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLibrary
{
    public static class Helper
    {
        public static string ToJson(this object source)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(source);
        }
        public static object ToObject(this string source, Type type)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(source, type);
        }
    }
}
