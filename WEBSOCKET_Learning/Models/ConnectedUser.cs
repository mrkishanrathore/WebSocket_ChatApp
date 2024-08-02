using Fleck;

namespace WEBSOCKET_Learning.Models
{
    public class ConnectedUser
    {
        public string UserName { get; set; } = "User";
        public Guid UserId { get; set; }
        public required IWebSocketConnection ws {  get; set; }
    }

    public class MsgData
    {
        public string Type { get; set; } = "";
        public string Message { get; set; } = string.Empty;
        public string UserName { get; set; } = String.Empty;
    }

    public class UserList
    {
        public string Type { get; set; } = "";
        public List<string> UserNames { get; set; } = new List<string>();
    }
}
