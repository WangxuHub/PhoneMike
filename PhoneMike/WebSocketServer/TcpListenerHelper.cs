
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace PhoneMike.WebSocketServer
{

    public class TcpListenerHelper
    {
      

        public static void RunServer(string address,int port)
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(address), port);
            listener.Stop();
            listener.Start();

            new System.Threading.Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        //Console.WriteLine("Waiting for a client to connect...");
                        TcpClient client = listener.AcceptTcpClient();
                        // Console.WriteLine("Client connected");
                        ProcessClient(client);
                    }
                    catch
                    {
                        ;
                    }
                }
            }).Start();
        }

        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        static void ProcessClient(TcpClient client)
        {
            SslStream sslStream = new SslStream(client.GetStream(), false, ValidateServerCertificate);

            NetworkStream nStream=client.GetStream();
            int length= 2048;//(int)nStream.Length;
            byte[] bytes=new byte[length];
            nStream.Read(bytes,0,length);
            string sss=System.Text.Encoding.UTF8.GetString(bytes);
            try
            {
                sslStream.AuthenticateAsServer(PhoneMike.Common.CryptHelper.currentX509Cert, false, SslProtocols.Tls, true);
                #region 显示客户端 详情
                //DisplaySecurityLevel(sslStream);
                //DisplaySecurityServices(sslStream);
                //DisplayCertificateInformation(sslStream);
                //DisplayStreamProperties(sslStream);
                #endregion
                sslStream.ReadTimeout = int.MaxValue;
                sslStream.WriteTimeout = int.MaxValue;
                Console.WriteLine("Server:Reading Message!");
              //  string messageData =
                ReadMessage(sslStream, client);
              //  Console.WriteLine("Received: {0}", messageData);
                byte[] message = Encoding.UTF8.GetBytes("Hello from the server.<EOF>");
                Console.WriteLine("Sending hello message.");
                sslStream.Write(message);
            }
            catch (AuthenticationException e)
            {
               // Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                ;//    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
              //  Console.WriteLine("Authentication failed - closing the connection.");
                sslStream.Close();
                client.Close();
                return;
            }
            finally
            {
                
            }
        }

        static void ReadMessage(SslStream sslStream,TcpClient client)
        {
            System.Threading.Thread thread = new System.Threading.Thread(() => {
                while (client.Connected)
                {
                    byte[] buffer = new byte[2048];
                    StringBuilder messageData = new StringBuilder();
                    int bytes = -1;
                    do
                    {
                        bytes = sslStream.Read(buffer, 0, buffer.Length);
                        Decoder decoder = Encoding.UTF8.GetDecoder();
                        char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                        decoder.GetChars(buffer, 0, bytes, chars, 0);
                        messageData.Append(chars);
                        if (messageData.ToString().IndexOf("<EOF>") != -1)
                        {
                            break;
                        }
                    }
                    while (bytes != 0);
                    Console.WriteLine("Client:{0}", messageData.ToString());

                    if (messageData.ToString ()== "exit")
                    {
                        sslStream.Close();
                        client.Close();
                    }
                }
            });
            thread.Start();

        }

        #region 显示证书详情
        static void DisplaySecurityLevel(SslStream stream)
        {
            Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            Console.WriteLine("Protocol: {0}", stream.SslProtocol);
        }

        static void DisplaySecurityServices(SslStream stream)
        {
            Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
            Console.WriteLine("IsSigned: {0}", stream.IsSigned);
            Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
        }

        static void DisplayStreamProperties(SslStream stream)
        {
            Console.WriteLine("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
            Console.WriteLine("Can timeout: {0}", stream.CanTimeout);
        }

        static void DisplayCertificateInformation(SslStream stream)
        {
            Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

            X509Certificate localCertificate = stream.LocalCertificate;
            if (stream.LocalCertificate != null)
            {
                Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                localCertificate.Subject,
                    localCertificate.GetEffectiveDateString(),
                    localCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Local certificate is null.");
            }
            X509Certificate remoteCertificate = stream.RemoteCertificate;
            if (stream.RemoteCertificate != null)
            {
                Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificate.Subject,
                    remoteCertificate.GetEffectiveDateString(),
                    remoteCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Remote certificate is null.");
            }
        }
        private static void DisplayUsage()
        {
            Console.WriteLine("To start the server specify:");
            Console.WriteLine("serverSync certificateFile.cer");
        }
        #endregion

       
    }
}
