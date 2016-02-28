using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
namespace SocketPost.Common
{
    public static class JsonHelper
    {
        public static string GetJsonValue(this object obj,string key)
        {
            if (!(obj is Newtonsoft.Json.Linq.JObject)) return "";
            
            Newtonsoft.Json.Linq.JObject jObject=obj as Newtonsoft.Json.Linq.JObject;

            if (jObject[key]==null) return "";

            return jObject[key].ToString();
            
        }

        public static string ToJson(this object json)
        {
            JsonSerializerSettings dateFormat = new JsonSerializerSettings();
         //   dateFormat.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            //dateFormat.DateFormatHandling = "yyyy-MM-dd HH:mm:ss";
            //dateFormat.DateFormatHandling=
            return Newtonsoft.Json.JsonConvert.SerializeObject(json, dateFormat);
        }
    }

    public class mJsonResult
    {
        public bool success = false;
        public string msg = "";
        public int pageIndex = 0;
        public int pageSize = 20;
        public int total = 0;
        public System.Collections.ICollection rows;
        
        /// <summary>
        /// 推送到客户端的推送类型，客户端根据此字段进行判断，不同的推送类型
        /// </summary>
        public string clientPostType; 
        public object obj;
    }
}