<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RsaTest.aspx.cs" Inherits="PhoneMike.Common.RsaTest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>

     sfgsdgds   <label id="lbl1" runat="server"></label>
        <asp:TextBox ID="TextBox1" runat="server" Height="133px" OnTextChanged="TextBox1_TextChanged" TextMode="MultiLine" Width="285px"></asp:TextBox>

        <br />
        <br />
        <br />
        <asp:Button ID="Button1" runat="server" Text="加密" OnClick="Button1_Click" />
        <br />

        <asp:TextBox ID="TextBox2" runat="server" Height="119px" TextMode="MultiLine" Width="280px"></asp:TextBox>

        <br />
        <asp:Button ID="Button2" runat="server" Text="解密" OnClick="Button2_Click" />
        <br />

        <asp:TextBox ID="TextBox3" runat="server" Height="106px" TextMode="MultiLine" Width="263px"></asp:TextBox>
    </div>
    </form>
</body>
</html>
