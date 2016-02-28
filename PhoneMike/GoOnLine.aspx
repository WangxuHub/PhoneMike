<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GoOnLine.aspx.cs" Inherits="SocketPost.SocketPost.GoOnLine" %>


<!DOCTYPE html>
<html>
<head runat="server">
<meta charset="UTF-8">
	<title>Web Socket Client</title>
</head>
<body style="padding:10px;">
	<h1>Web Socket Chating Room</h1>
	<div style="margin:5px 0px;">
		Address:
&nbsp;<div><input id="address" type="text" value="<%=PhoneMike.WebSocketServer.WebSocketInfo.WebSocketAdress %>" style="width:400px;"/></div>
	</div>
	<div style="margin:5px 0px;">
		Name:
		<div><input id="name" type="text" value="Byron" style="width:400px;"/></div>
	</div>
	<div>
		<button id="connect" onclick="connect();">connect server</button> &nbsp;&nbsp;
		<button id="disconnect" onclick="quit();">disconnect</button>&nbsp;&nbsp;
		<button id="clear" onclick="clearMsg();">clear</button>
	  
	</div>
	<h5 style="margin:4px 0px;">Message:</h5>
	<div id="message" style="border:solid 1px #333; padding:4px; width:80%; overflow:auto;
	 	background-color:#404040; height:300px; margin-bottom:8px; font-size:14px;">
	</div>
	<input id="text" type="text" onkeypress="enter(event);" style="width:340px"/> &nbsp;&nbsp;
	<button id="send" onclick="send();">send</button>

<script type="text/javascript">
var name = document.getElementById('name').value;
var msgContainer = document.getElementById('message');
var text = document.getElementById('text');

function connect() {
	var address = document.getElementById('address').value;

	if ((!!window.ws) && window.ws.url == address)
    {
        var msg = document.createElement('div');
	    msg.style.color = '#0f0';
	    msg.innerHTML = "已建立指定连接";
	    msgContainer.appendChild(msg);
	    msgContainer.scrollTop = msgContainer.scrollHeight - msgContainer.clientHeight;
        return ;
    }
	ws = new WebSocket(address);
	ws.onopen = function (e) {
	    var requestData = new Object();
	    requestData.SocketPostType = "deviceGoOnLine";
	    requestData.DeviceSN = name+Math.random();

	    var msg = document.createElement('div');
	    msg.style.color = '#0f0';
	    msg.innerHTML = "发送信息:"+JSON.stringify(requestData);
	    msgContainer.appendChild(msg);
	    msgContainer.scrollTop = msgContainer.scrollHeight - msgContainer.clientHeight;


	    ws.send(JSON.stringify(requestData));
	};
	ws.onmessage = function (e) {
	    var msg = document.createElement('div');
	    msg.style.color = '#fff';
	    msg.innerHTML = e.data;
	    msgContainer.appendChild(msg);
	    msgContainer.scrollTop = msgContainer.scrollHeight - msgContainer.clientHeight;

	    var json = JSON.parse(e.data);
	    switch (json.clientPostType) {
	        case "retDeviceGoOnLine":
	            retDeviceGoOnLine(json);
	            break;
	    }




	};
	ws.onerror = function (e) {
	    var msg = document.createElement('div');
	    msg.style.color = '#0f0';
	    msg.innerHTML = 'Server > ' + e.data;
	    msgContainer.appendChild(msg);
	    msgContainer.scrollTop = msgContainer.scrollHeight - msgContainer.clientHeight;
	};
	ws.onclose = function (e) {
	    var msg = document.createElement('div');
	    msg.style.color = '#0f0';
	    msg.innerHTML = "Server > connection closed by server.";
	    msgContainer.appendChild(msg);
	    msgContainer.scrollTop = msgContainer.scrollHeight - msgContainer.clientHeight;

	    if (ws) {
	        ws.close();
	        ws = null;
	    }

	};
	text.focus();
}

function retDeviceGoOnLine(json) {
    //$.messager.alert('提示', json.msg);    
}

function quit() {
	if (ws) {
	    ws.close();
	    var msg = document.createElement('div');
	    msg.style.color = '#0f0';
	    msg.innerHTML = 'Server > connection closed.';
	    msgContainer.appendChild(msg);
	    msgContainer.scrollTop = msgContainer.scrollHeight - msgContainer.clientHeight;
	    ws = null;
	}
}

function send() {
	ws.send(text.value);
	setTimeout(function () {
	    //	msgContainer.scrollTop=msgContainer.getBoundingClientRect().height;
	}, 100);
	text.value = '';
	text.focus();
}

function clearMsg() {
	msgContainer.innerHTML = "";
}

function enter(event) {
	if (event.keyCode == 13) {
	    send();
	}
}
</script>
</body>
</html>
