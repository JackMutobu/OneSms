using System.Collections.Generic;

namespace OneSms.Online.Services
{
    public class ServerConnectionService
    {
        public ServerConnectionService()
        {
            ConnectedServers = new Dictionary<string, string>();
            ConnectedServersReverse = new Dictionary<string, string>();
        }
        public Dictionary<string,string> ConnectedServers { get; set; }

        public Dictionary<string, string> ConnectedServersReverse { get; set; }
    }
}
