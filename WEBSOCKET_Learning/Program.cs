using Fleck;
using System.Text.Json;
using WEBSOCKET_Learning.Models;

var server = new WebSocketServer("ws://0.0.0.0:8181");
var wsConnections = new List<ConnectedUser>();

server.Start(ws =>
{

    ws.OnOpen = () =>
    {
        var id = Guid.NewGuid();
        ConnectedUser user = new ConnectedUser()
        {
            UserId = id,
            ws = ws,
            UserName = "User"+id
        };

        wsConnections.Add(user);
        sendUserList();
    };

    ws.OnClose = () =>
    {
        if (GetUserWithWs(ws) != null)
        {
            wsConnections.Remove(GetUserWithWs(ws));
        }
        sendUserList();
    };

    ws.OnMessage = (message) =>
    {
        if (message != null)
        {
            Console.WriteLine(message);
            var data = JsonSerializer.Deserialize<Dictionary<string, string>?>(message);
            string type = data["type"];
            if (data != null && type != null)
            {
                switch (type)
                {
                    case "username":
                        if (data.ContainsKey("username"))
                        {
                            ConnectedUser? user = GetUserWithWs(ws);
                            if (user != null)
                            {
                                user.UserName = data["username"];
                                sendUserList();
                            }
                        }
                        break;

                    case "msg":
                        if (data.ContainsKey("msg"))
                        {
                            ConnectedUser? user = GetUserWithWs(ws);
                            var MsgData = new MsgData()
                            {
                                Type = type,
                                Message = data["msg"],
                                UserName = user.UserName,
                            };
                            var msgJson = JsonSerializer.Serialize(MsgData);
                            foreach (var wsc in wsConnections)
                            {
                                if(wsc.ws != ws)
                                {
                                    Console.WriteLine(msgJson);
                                    wsc.ws.Send(msgJson);
                                }
                            }
                        }
                        break;
                    
                    case "msgOne":
                        if (data.ContainsKey("msg") && data.ContainsKey("sendTo"))
                        {
                            ConnectedUser? user = GetUserWithWs(ws);
                            var MsgData = new MsgData()
                            {
                                Type= "msg",
                                Message = data["msg"],
                                UserName = user.UserName,
                            };
                            ConnectedUser? sendTo = GetUserWithName(data["sendTo"]);
                            var msgJson = JsonSerializer.Serialize(MsgData);
                            if (sendTo != null)
                            {
                                sendTo.ws.Send(msgJson);
                            }
                        }
                        break;
                }
            }
        }
        
    };
});


void sendUserList()
{
    List<string> userList = new List<string>();

    foreach (var wsc in wsConnections)
    {
        userList.Add(wsc.UserName);
    }

    var MsgData = new UserList()
    {
        Type = "updatedUsersList",
        UserNames = userList
    };

    var msgJson = JsonSerializer.Serialize(MsgData);
    foreach (var wsc in wsConnections)
    {
        wsc.ws.Send(msgJson);
    }
}

ConnectedUser? GetUserWithWs(IWebSocketConnection connection)
{
    ConnectedUser? user = wsConnections.Find(e => e.ws == connection);
    return user;
}

ConnectedUser? GetUserWithName(string name)
{
    ConnectedUser? user = wsConnections.Find(e => e.UserName == name);
    return user;
}

WebApplication.CreateBuilder(args).Build().Run();
