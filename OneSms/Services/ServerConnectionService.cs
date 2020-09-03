using System.Collections.Generic;
using System.Reactive.Subjects;

namespace OneSms.Services
{
    public interface IServerConnectionService
    {
        Dictionary<string, string> ConnectedServers { get; set; }
        Dictionary<string, string> ConnectedServersReverse { get; set; }

        public Subject<string> OnServerConnected { get; set; }
        public Subject<string> OnServerDisconnected { get; set; }
    }

    public class ServerConnectionService : IServerConnectionService
    {
        public ServerConnectionService()
        {
            ConnectedServers = new Dictionary<string, string>();
            ConnectedServersReverse = new Dictionary<string, string>();
            OnServerConnected = new Subject<string>();
            OnServerDisconnected = new Subject<string>();
        }
        public Dictionary<string, string> ConnectedServers { get; set; }

        public Dictionary<string, string> ConnectedServersReverse { get; set; }

        public Subject<string> OnServerConnected { get; set; }

        public Subject<string> OnServerDisconnected { get; set; }
    }
}
