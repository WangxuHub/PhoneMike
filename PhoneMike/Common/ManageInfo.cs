using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocketPost.Common
{
    public class ManageInfo : PhoneMike.WebSocketServer.ClientInfo
    {
        public string userName;

        //上线时间
        public DateTime onLineTime;
    }
}