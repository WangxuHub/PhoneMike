using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PhoneMike.WebSocketServer;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using SocketPost.Common;
namespace PhoneMike.WebSocketServer
{

    public class WebSocket : PhoneMike.WebSocketServer.WebSocketHelper
    {
        //在线设备列表
        public static Dictionary<Socket, DeviceInfo> devicePool = new Dictionary<Socket, DeviceInfo>();

        //在线管理页面列表
        public static Dictionary<Socket, ManageInfo> manageInfoPool = new Dictionary<Socket, ManageInfo>();
        private object socketContext;
        private Socket curClientSocket;

        public override void MakeReturnContent(string msg, Socket client)
        {
            curClientSocket = client;
            socketContext = Newtonsoft.Json.JsonConvert.DeserializeObject(msg);
            string socketPostType = socketContext.GetJsonValue("SocketPostType");

            switch (socketPostType)
            {
                case "deviceGoOnLine":
                    DeviceGoOnLine();//设备上线请求
                    break;
                case "getOnlineDeviceList":
                    GetOnlineDeviceList();//获取在线设备
                    break;
                case "manageGoOnLine":
                    ManageGoOnLine();//控制页面上线请求
                    break;
                case "getOnlineManageList":
                    GetOnlineManageList();//获取在线控制页面
                    break;
                default :
                    break;
            }

        }

        public override bool IsCloseSocket(Socket client)
        {
            if ((clientPool[client].buffer[0] | 0xf8) == 0xf8)
            {
                //把客户端标记为关闭，并在clientPool中清除
                if (client != null && client.Connected)
                {
                    //关闭Socket之前，首选需要把双方的Socket Shutdown掉
                    client.Shutdown(SocketShutdown.Both);
                    //Shutdown掉Socket后主线程停止10ms，保证Socket的Shutdown完成
                    System.Threading.Thread.Sleep(10);
                    //关闭客户端Socket,清理资源
                    client.Close();

                    Console.WriteLine("Client {0} disconnet", clientPool[client].Name);
                    clientPool.Remove(client);

                    devicePool.Remove(client);
                    manageInfoPool.Remove(client);

                    GetOnlineDeviceList();
                }
                return true;
            }
            return false;
        }



        #region 设备上线请求
        private void DeviceGoOnLine()
        {
            string deviceSN = socketContext.GetJsonValue("DeviceSN");
            DeviceInfo clientInfo = new DeviceInfo()
            { 
                deviceSN=deviceSN,
                onLineTime=DateTime.Now
            };


            devicePool.Add(curClientSocket,clientInfo);

            mJsonResult json = new mJsonResult()
            {
                success = true,
                msg=string.Format("上线成功，上线时间:{0}",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                clientPostType = "retDeviceGoOnLine"
            };

            SocketMessage sm = new SocketMessage()
            {
                Message = json.ToJson(),
                SendToClients=new Dictionary<Socket,ClientInfo>(){{curClientSocket,null}}
            };
            msgPool.Add(sm);

            //设备上线 推送给所有管理页面
            GetOnlineDeviceList();

        }
        #endregion

        #region 管理页面上线请求
        private void ManageGoOnLine()
        {
            string userName = socketContext.GetJsonValue("userName");
            ManageInfo clientInfo = new ManageInfo()
            {
                userName = userName,
                onLineTime = DateTime.Now
            };


            manageInfoPool.Add(curClientSocket, clientInfo);

            mJsonResult json = new mJsonResult()
            {
                success = true,
                msg = string.Format("上线成功，上线时间:{0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                clientPostType = "retManageGoOnLine"
            };

            SocketMessage sm = new SocketMessage()
            {
                Message = json.ToJson(),
                SendToClients = new Dictionary<Socket, ClientInfo>() { { curClientSocket, null } }
            };
            msgPool.Add(sm);
        }
        #endregion

        #region 获取在线设备
        private void GetOnlineDeviceList()
        {
            var deviceList=(from  item in devicePool
                           select item.Value).ToList();


            mJsonResult json = new mJsonResult()
            {
                success = true,
                rows = deviceList,
                clientPostType = "retGetOnlineDeviceList"
            };

            //推送给所有连接的管理页面
            Dictionary<Socket, ClientInfo> sendto = (
                from a in manageInfoPool
                select new KeyValuePair<Socket, ClientInfo>(a.Key, null)
                ).ToDictionary(key=>key.Key,value=>value.Value);

            SocketMessage sm = new SocketMessage()
            {
                Message = json.ToJson(),
                SendToClients = sendto
            };
            msgPool.Add(sm);
        }
        #endregion

        #region 获取在线的控制页面
        private void GetOnlineManageList()
        {
            var manageList = (from item in manageInfoPool
                              select item.Value).ToList();

            mJsonResult json = new mJsonResult()
            {
                success = true,
                rows = manageList,
                clientPostType = "retGetOnlineManageList"
            };

            //推送给所有连接的管理页面
            Dictionary<Socket, ClientInfo> sendto = (
                from a in manageInfoPool
                select new KeyValuePair<Socket, ClientInfo>(a.Key, null)
                ).ToDictionary(key => key.Key, value => value.Value);

            SocketMessage sm = new SocketMessage()
            {
                Message = json.ToJson(),
                SendToClients = sendto
            };
            msgPool.Add(sm);
        }
        #endregion
    }


    public static class WebSocketInfo
    {
        public static string WebSocketAdress
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["webSocketAdress"];
            }
        }

        public static string WebSocketAdressIP
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["webSocketAdressIP"];
            }
        }


        public static string WebSocketAdressPort
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["webSocketAdressPort"];
            }
        }
    }
        


    
}