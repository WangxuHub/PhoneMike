<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="枚举.aspx.cs" Inherits="PhoneMike.WebSocketServer.枚举" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <script>
        var test =
//(
     function (a) {

         this.a = a;

         return function (b) {
             return this.a + b;
         }

     }(1)
        //);

        var test=function (b)
        {
            return 1+b；
        }

        console.log(test(4));

    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Button" />
    
    </div>
    </form>
</body>
</html>
