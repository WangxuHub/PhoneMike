using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PhoneMike.Common
{
    public partial class RsaTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.lbl1.InnerHtml= Convert.ToString( Session["code"]);
        }

        protected void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }  

        //加密
        protected void Button1_Click(object sender, EventArgs e)
        {
            Session["code"] = TextBox1.Text;
            string unCryptStr = TextBox1.Text;
            string cryptStr =Common.CryptHelper.RsaEncrypt(unCryptStr);;// Common.CryptHelper.RsaEncrypt(unCryptStr);

            TextBox2.Text = cryptStr;
        }

        protected void Button2_Click(object sender, EventArgs e)
        {

            string cryptStr = TextBox2.Text;
            string unCryptStr = Common.CryptHelper.RsaDecrypt(cryptStr); ;//Common.CryptHelper.RsaDecrypt(cryptStr);

            TextBox3.Text = unCryptStr;
        }
    }  
}