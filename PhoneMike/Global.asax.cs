using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace PhoneMike
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            string strPort = WebSocketServer.WebSocketInfo.WebSocketAddressPort;

            PhoneMike.WebSocketServer.WebSocket helper = new PhoneMike.WebSocketServer.WebSocket();
            helper.Run(int.Parse(strPort));

            string portSecure = WebSocketServer.WebSocketInfo.WebSocketAddressPortSecure;
            WebSocketServer.TcpListenerHelper.RunServer(WebSocketServer.WebSocketInfo.WebSocketAdressIP,Convert.ToInt32(portSecure));
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}