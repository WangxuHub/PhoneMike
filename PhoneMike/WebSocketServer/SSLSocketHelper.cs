using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace PhoneMike.WebSocketServer
{
    public class SSLSocketHelper
    {
        public  static Dictionary<Socket, ClientInfo> clientPool = new Dictionary<Socket, ClientInfo>();
        public  static List<SocketMessage> msgPool = new List<SocketMessage>();

        /// <summary>
        /// 启动服务器，监听客户端请求
        /// </summary>
        /// <param name="port">服务器端进程口号</param>
        public void Run(int port)
        {
            Thread serverSocketThraed = new Thread(() =>
            {
             
                    //Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    Socket server = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    string ipAddress = WebSocketInfo.WebSocketAdressIP;
                    server.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));
                    server.Listen(10);

                    //当中第二个参数为Object 可以放任何值类型（装箱），再委托方法中IAsyncResult.AsyncState，就是已装箱的值，执行相应的拆箱操作就可以完成，委托传值
                    server.BeginAccept(new AsyncCallback(Accept), server);
                //}
                //catch { ;}
            });

            serverSocketThraed.Start();

            Broadcast();
        }

        /// <summary>
        /// 在独立线程中不停地向所有客户端广播消息
        /// </summary>
        private void Broadcast()
        {
            Thread broadcast = new Thread(() =>
            {
                while (true)
                {
                    if (msgPool.Count > 0)
                    {
                        SocketMessage sm = msgPool[0];
                      

                        try
                        {
                            byte[] msgByte = PackageServerData(sm.Message);
                            foreach (KeyValuePair<Socket, ClientInfo> cs in sm.SendToClients)
                            {
                                Socket client = cs.Key;
                                if (client.Poll(10, SelectMode.SelectWrite))
                                {

                                    client.Send(msgByte, msgByte.Length, SocketFlags.None);

                                }
                            }
                        }
                        catch
                        {
                        }
                        msgPool.RemoveAt(0);
                    }
                }
            });

            broadcast.Start();
        }

        /// <summary>
        /// 处理客户端连接请求,成功后把客户端加入到clientPool
        /// </summary>
        /// <param name="result">Result.</param>
        private void Accept(IAsyncResult result)
        {
            Socket server = result.AsyncState as Socket;
            Socket client = server.EndAccept(result);
            try
            {
                //处理下一个客户端连接
                server.BeginAccept(new AsyncCallback(Accept), server);
                byte[] buffer = new byte[65536];

                ClientInfo info = new ClientInfo();
                info.Id = client.RemoteEndPoint;
                info.handle = client.Handle;
                info.buffer = buffer;
                //把客户端存入clientPool
                if(!clientPool.ContainsKey(client))
                    clientPool.Add(client, info);

                //接收客户端消息
                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(Recieve), client);
               
            }
            catch //(Exception ex)
            {
            }
        }

        #region 关闭连接相关代码
        public virtual bool IsCloseSocket(Socket client)
        {
            if ((clientPool[client].buffer[0] | 0xf8) == 0xf8)
            {
                CloseClient(client);
                return true;
            }
            return false;
        }

        private void CloseClient(Socket client)
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
            }
        }
        #endregion

        /// <summary>
        /// 处理客户端发送的消息，接收成功后加入到msgPool，等待广播
        /// </summary>
        /// <param name="result">Result.</param>
        private void Recieve(IAsyncResult result)
        {
            Socket client = result.AsyncState as Socket;

            if (client == null || !clientPool.ContainsKey(client))
            {
                return;
            }

            try
            {
                int length = client.EndReceive(result);
                if(length==0)
                {
                    CloseClient(client);
                }
                byte[] buffer = clientPool[client].buffer;

                //接收消息
                client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(Recieve), client);

                //=====分析数据帧=================================================================================================================
                string msg = Encoding.UTF8.GetString(buffer, 0, length);
                AnalysisSSLDataFrame(buffer,length);
                //===================================================================================================================

                //与客户端的业务无关，只是客户端发起的websocket建立连接请求时，自带的数据帧。
                if (!clientPool[client].IsHandShaked && msg.Contains("Sec-WebSocket-Key"))
                {
                    client.Send(PackageHandShakeData(buffer, length));
                    clientPool[client].IsHandShaked = true;
                    return;
                }


                if (IsCloseSocket(client)) return;

                //与客户端的业务有关，是WebSocket特有的数据帧格式 解析客户端发送来的数据 
                msg = AnalyzeClientData(buffer, length);

                MakeReturnContent(msg, client);


            }
            catch
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

                    clientPool.Remove(client);
                }
            }
        }

        public  virtual void MakeReturnContent(string msg, Socket client)
        {
             
        }

        /// <summary>
        /// 分析是
        /// </summary>
        /// <param name="?"></param>
        /// <param name="?"></param>
        private void AnalysisSSLDataFrame(byte[] buffer, int length)
        {
            //SSL握手协议报文头包括三个字段：
            //类型（1字节）：该字段指明使用的SSL握手协议报文类型。SSL握手协议报文包括10种类型。报文类型见图13.5。
            //长度（3字节）：以字节为单位的报文长度。
            //内容（≥1字节）：使用的报文的有关参数。

            byte byteType = buffer[0];
            byte[] byteLength =new Byte[3];
         //   buffer.CopyTo(byteLength, 1);
            Array.Copy(buffer, 1, byteLength, 0, 3);

            byte [] byteContent=new byte[length-4];
            //buffer.CopyTo(byteContent,4);
            Array.Copy(buffer, 4, byteContent, 0, length - 4);

            string msg = System.Text.Encoding.UTF8.GetString(byteContent);



        }



        #region 与WebSocket帧数据相关，包含接收与发送的数据帧处理

        /// <summary>
        /// 打包服务器握手数据
        /// </summary>
        /// <returns>The hand shake data.</returns>
        /// <param name="handShakeBytes">Hand shake bytes.</param>
        /// <param name="length">Length.</param>
        private byte[] PackageHandShakeData(byte[] handShakeBytes, int length)
        {
            string handShakeText = Encoding.UTF8.GetString(handShakeBytes, 0, length);
            string key = string.Empty;
            Regex reg = new Regex(@"Sec\-WebSocket\-Key:(.*?)\r\n");
            Match m = reg.Match(handShakeText);
            if (m.Value != "")
            {
                key = Regex.Replace(m.Value, @"Sec\-WebSocket\-Key:(.*?)\r\n", "$1").Trim();
            }

            byte[] secKeyBytes = SHA1.Create().ComputeHash(
                                     Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
            string secKey = Convert.ToBase64String(secKeyBytes);

            var responseBuilder = new StringBuilder();
            responseBuilder.Append("HTTP/1.1 101 Switching Protocols" + "\r\n");
            responseBuilder.Append("Upgrade: websocket" + "\r\n");
            responseBuilder.Append("Connection: Upgrade" + "\r\n");
            responseBuilder.Append("Sec-WebSocket-Accept: " + secKey + "\r\n\r\n");

            return Encoding.UTF8.GetBytes(responseBuilder.ToString());
        }

        /// <summary>
        /// 解析客户端发送来的数据
        /// </summary>
        /// <returns>The data.</returns>
        /// <param name="recBytes">Rec bytes.</param>
        /// <param name="length">Length.</param>
        private string AnalyzeClientData(byte[] recBytes, int length)
        {
            if (length < 2)
            {
                return string.Empty;
            }

            bool fin = (recBytes[0] & 0x80) == 0x80; // 1bit，1表示最后一帧  
            if (!fin)
            {
                return string.Empty;// 超过一帧暂不处理 
            }

            bool mask_flag = (recBytes[1] & 0x80) == 0x80; // 是否包含掩码  
            if (!mask_flag)
            {
                return string.Empty;// 不包含掩码的暂不处理
            }

            int payload_len = recBytes[1] & 0x7F; // 数据长度  

            byte[] masks = new byte[4];
            byte[] payload_data;

            if (payload_len == 126)
            {
                Array.Copy(recBytes, 4, masks, 0, 4);
                payload_len = (UInt16)(recBytes[2] << 8 | recBytes[3]);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 8, payload_data, 0, payload_len);

            }
            else if (payload_len == 127)
            {
                Array.Copy(recBytes, 10, masks, 0, 4);
                byte[] uInt64Bytes = new byte[8];
                for (int i = 0; i < 8; i++)
                {
                    uInt64Bytes[i] = recBytes[9 - i];
                }
                UInt64 len = BitConverter.ToUInt64(uInt64Bytes, 0);

                payload_data = new byte[len];
                for (UInt64 i = 0; i < len; i++)
                {
                    payload_data[i] = recBytes[i + 14];
                }
            }
            else
            {
                Array.Copy(recBytes, 2, masks, 0, 4);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 6, payload_data, 0, payload_len);

            }

            for (var i = 0; i < payload_len; i++)
            {
                payload_data[i] = (byte)(payload_data[i] ^ masks[i % 4]);
            }

            return Encoding.UTF8.GetString(payload_data);
        }

        /// <summary>
        /// 把发送给客户端消息打包处理
        /// </summary>
        /// <returns>The data.</returns>
        /// <param name="message">Message.</param>
        private byte[] PackageServerData(string  msg)
        {
           
            byte[] content = null;
            byte[] temp = Encoding.UTF8.GetBytes(msg.ToString());

            if (temp.Length < 126)
            {
                content = new byte[temp.Length + 6];
                content[0] = 0x81;
                content[1] = (byte)(temp.Length); //已经加上掩码

                Array.Copy(temp, 0, content, 2, temp.Length);
            }
            else if (temp.Length < 0xFFFF)
            {
                content = new byte[temp.Length + 4];
                content[0] = 0x81;
                content[1] = 126;

                content[3] = (byte)(temp.Length & 0xFF);
                content[2] = (byte)(temp.Length >> 8 & 0xFF);

                Array.Copy(temp, 0, content, 4, temp.Length);
            }
            else
            {
                // 暂不处理超长内容  
            }

            return content;
        }

        #endregion
    }

}
