using Microsoft.AspNetCore.SignalR;
using OneSms.Contracts.V1;
using OneSms.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace OneSms.Services
{
    public interface IServerConnectionService
    {
        Dictionary<string, string> ConnectedServers { get; set; }

        void AddServer(string serverId, string connectionId);
        void RemoveServer(string connectionId);
        Task<bool> CheckClientAvailability(string connectionId);
        void CheckClientAvailabilityCallback(Guid requestId, bool response);

        
    }

    public class ServerConnectionService : IServerConnectionService
    {
        private readonly IHubContext<OneSmsHub> _hubContext;
        private ConcurrentDictionary<Guid, object> _pendingTasks;

        public ServerConnectionService(IHubContext<OneSmsHub> hubContext)
        {
            ConnectedServers = new Dictionary<string, string>();
            ConnectedServersReverse = new Dictionary<string, string>();
            _hubContext = hubContext;
            _pendingTasks = new ConcurrentDictionary<Guid, object>();

            Observable.Interval(TimeSpan.FromSeconds(30))
             .Subscribe(async _ =>
             {
                 foreach(var conId in ConnectedServers.Values)
                 {
                     await CheckClientAvailability(conId);
                 }
             },ex => Debug.WriteLine(ex.Message));
        }
        public Dictionary<string, string> ConnectedServers { get; set; }

        public Dictionary<string, string> ConnectedServersReverse { get; set; }


        public void RemoveServer(string connectionId)
        {
            if(ConnectedServersReverse.TryGetValue(connectionId, out string? serverId))
            {
                ConnectedServers.Remove(serverId);
                ConnectedServersReverse.Remove(connectionId);
            }
        }

        public void AddServer(string serverId,string connectionId)
        {
            if (ConnectedServers.ContainsKey(serverId))
                ConnectedServers[serverId] = connectionId;
            else
                ConnectedServers.Add(serverId, connectionId);

            if (ConnectedServersReverse.ContainsKey(connectionId))
                ConnectedServersReverse[connectionId] = serverId;
            else
                ConnectedServersReverse.Add(connectionId, serverId);
        }

       

        public async Task<bool> CheckClientAvailability(string connectionId)
        {
            var requestId = Guid.NewGuid();
            try
            {
                var source = new TaskCompletionSource<bool>();
                var cancelTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                cancelTokenSource.Token.Register(() => source.TrySetCanceled(), useSynchronizationContext: false);
                _pendingTasks[requestId] = source;
            await _hubContext.Clients.Client(connectionId).SendAsync(SignalRKeys.CheckClientAlive, nameof(CheckClientAvailabilityCallback), requestId);

                return await source.Task;
            }
            catch(TaskCanceledException ex)
            {
                var taskWasCancelled = ex.Message;
                if (_pendingTasks.TryRemove(requestId, out var canceledTask))
                    RemoveServer(connectionId);
            }
            catch(Exception ex)
            {
                var otherException = ex.Message;
            }
            return false;
        }

        public void CheckClientAvailabilityCallback(Guid requestId, bool response)
        {
            if (_pendingTasks.TryRemove(requestId, out var obj) && obj is TaskCompletionSource<bool> source)
                source.SetResult(response);
        }

    }
}
