using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace PhoneMike.Common
{
    public class CryptHelper
    {

        /// <summary>
        /// RSA加密，从xml 公钥 加密
        /// </summary>
        /// <param name="publickey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RSAEncryptFromXml(string content)
        {
            string publickey = @"<RSAKeyValue><Modulus>5m9m14XH3oqLJ8bNGw9e4rGpXpcktv9MSkHSVFVMjHbfv+SJ5v0ubqQxa5YjLN4vc49z7SVju8s0X4gZ6AzZTn06jzWOgyPRV54Q4I0DCYadWW4Ze3e+BOtwgVU1Og3qHKn8vygoj40J6U85Z/PTJu3hN1m75Zr195ju7g9v4Hk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(publickey);
            cipherbytes = rsa.Encrypt(Encoding.UTF8.GetBytes(content), false);

            return Convert.ToBase64String(cipherbytes);
        }

        /// <summary>
        /// RSA解密，从xml 秘钥 解密
        /// </summary>
        /// <param name="privatekey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RSADecryptFromXml(string content)
        {
            string privatekey = @"<RSAKeyValue><Modulus>5m9m14XH3oqLJ8bNGw9e4rGpXpcktv9MSkHSVFVMjHbfv+SJ5v0ubqQxa5YjLN4vc49z7SVju8s0X4gZ6AzZTn06jzWOgyPRV54Q4I0DCYadWW4Ze3e+BOtwgVU1Og3qHKn8vygoj40J6U85Z/PTJu3hN1m75Zr195ju7g9v4Hk=</Modulus><Exponent>AQAB</Exponent><P>/hf2dnK7rNfl3lbqghWcpFdu778hUpIEBixCDL5WiBtpkZdpSw90aERmHJYaW2RGvGRi6zSftLh00KHsPcNUMw==</P><Q>6Cn/jOLrPapDTEp1Fkq+uz++1Do0eeX7HYqi9rY29CqShzCeI7LEYOoSwYuAJ3xA/DuCdQENPSoJ9KFbO4Wsow==</Q><DP>ga1rHIJro8e/yhxjrKYo/nqc5ICQGhrpMNlPkD9n3CjZVPOISkWF7FzUHEzDANeJfkZhcZa21z24aG3rKo5Qnw==</DP><DQ>MNGsCB8rYlMsRZ2ek2pyQwO7h/sZT8y5ilO9wu08Dwnot/7UMiOEQfDWstY3w5XQQHnvC9WFyCfP4h4QBissyw==</DQ><InverseQ>EG02S7SADhH1EVT9DD0Z62Y0uY7gIYvxX/uq+IzKSCwB8M2G7Qv9xgZQaQlLpCaeKbux3Y59hHM+KpamGL19Kg==</InverseQ><D>vmaYHEbPAgOJvaEXQl+t8DQKFT1fudEysTy31LTyXjGu6XiltXXHUuZaa2IPyHgBz0Nd7znwsW/S44iql0Fen1kzKioEL3svANui63O3o5xdDeExVM6zOf1wUUh/oldovPweChyoAdMtUzgvCbJk1sYDJf++Nr0FeNW1RB1XG30=</D></RSAKeyValue>";
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(privatekey);
            cipherbytes = rsa.Decrypt(Convert.FromBase64String(content), false);

            return Encoding.UTF8.GetString(cipherbytes);
        }


        /// <summary>
        /// Rsa 对象类型
        /// </summary>
        public enum eRsaProvideTpye
        {
            /// <summary>
            /// 公钥Rsa对象类型
            /// </summary>
            publicProvideTpye,

            /// <summary>
            /// 私钥Rsa对象类型
            /// </summary>
            privateProvideTpye
        }


        #region 当前证书 X509 currentX509Cert
        /// <summary>
        /// 当前证书 X509
        /// </summary>
        public static X509Certificate2 currentX509Cert
        {
            get
            {  //StoreName.My 为证书 ---个人  ，如果想要获取其他路径下的证书,如“受信任的根证书颁发机构”
                X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = store.Certificates;
                X509Certificate2 cert = null;

                foreach (X509Certificate2 c in certCollection)
                {
                    //if (c.SerialNumber == "295B5F13293C2A9A474491EC94851F93")
                    //{
                    //    cert = c;

                    //    break;
                    //}


                    if (c.SubjectName.Name.IndexOf("wangxuhub.com")>-1)
                    {
                        cert = c;

                        break;
                    }
                }

                store.Close();
                return cert;
            }
        }
        #endregion

        #region 当前证书 Rsa公钥 对象 currentPublicRsaProvide
        /// <summary>
        /// 当前证书 Rsa公钥 对象
        /// </summary>
        private static RSACryptoServiceProvider currentPublicRsaProvide
        {
            get
            {
                return (RSACryptoServiceProvider)currentX509Cert.PublicKey.Key;
            }
        }
        #endregion




        #region 当前证书 Rsa私钥 对象 currentPrivateRsaProvide
        /// <summary>
        /// 当前证书 Rsa私钥 对象
        /// </summary>
        private static RSACryptoServiceProvider currentPrivateRsaProvide
        {
            get
            {
                return (RSACryptoServiceProvider)currentX509Cert.PrivateKey;
            }
        }
        #endregion

        private static RSACryptoServiceProvider GetRsaProvide(eRsaProvideTpye rsaType)
        {
          

            RSACryptoServiceProvider provider=new RSACryptoServiceProvider();
            //从证书中获得RSAdCryptoServiceProvider
            switch (rsaType)
            {
                case eRsaProvideTpye.publicProvideTpye:
                    provider = currentPublicRsaProvide;
                    break;
                case eRsaProvideTpye.privateProvideTpye:
                    provider = currentPrivateRsaProvide;
                    break;
                default:
                    break;
            }
            //私钥 对象里面已经有公钥的信息了，
            //可以通过公钥和私钥加密，但是私钥进行解密 
            return provider;
        }

        /// <summary>
        /// Rsa加密 从公钥证书中
        /// </summary>
        /// <param name="unCryptString"></param>
        /// <returns></returns>
        public static string RsaEncrypt(string unCryptString)
        {
            // RSACryptoServiceProvider provider = GetRsaPrivateProvide();
            RSACryptoServiceProvider provider = GetRsaProvide(eRsaProvideTpye.publicProvideTpye);
            byte[] buff = Encoding.UTF8.GetBytes(unCryptString);


            byte[] retByte = provider.Encrypt(buff, false);

            return Convert.ToBase64String(retByte);
        }

        /// <summary>
        /// Rsa解密 从公钥证书中
        /// </summary>
        /// <param name="CryptString"></param>
        /// <returns></returns>
        public static string RsaDecrypt(string CryptString)
        {
            // RSACryptoServiceProvider provider = GetRsaPublicProvide();
            RSACryptoServiceProvider provider = GetRsaProvide(eRsaProvideTpye.privateProvideTpye);
            byte[] buff = Convert.FromBase64String(CryptString);

            byte[] retByte = provider.Decrypt(buff, false);

            return Encoding.UTF8.GetString(retByte);

        }


        private static RSAParameters paramsters
        {
            get
            {
                return currentPrivateRsaProvide.ExportParameters(true);
            }
        }


    }
}