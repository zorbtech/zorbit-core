namespace Stratis.Bitcoin.P2P.Peer
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Stratis.Bitcoin.Configuration;
    using DotNetTor;

    public class NetworkHandler
    {
        private NodeSettings NodeSettings;
        private TotClient TotClient
        {
            get
            {
                if (TotClients.Count == 0)
                {
                    return null;
                }
                var address = this.RemoteEndPoint.Address.ToString();
                if (TotClients.ContainsKey(address))
                {
                    TotClient client = null;
                    TotClients.TryGetValue(address, out client);
                    return client;
                }
                else
                {
                    return null;
                }
            }
        }

        private static ConcurrentDictionary<string, TotClient> TotClients = new ConcurrentDictionary<string, TotClient>();

        public TcpClient TcpClient { get; private set; }

        public NetworkStream Stream
        {
            get
            {
                return this.TcpClient.Connected ? this.TcpClient.GetStream() : null;
            }
        }

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                return this._endPoint;
            }
            private set
            {
                if (value.Address.IsIPv4MappedToIPv6)
                {
                    value.Address = value.Address.MapToIPv4();
                }
                this._endPoint = value;
            }
        }

        public bool HasInboundClient { get; private set; }

        private IPEndPoint _endPoint;

        public NetworkHandler(NodeSettings nodeSettings)
        {
            this.NodeSettings = nodeSettings;
            this.TcpClient = new TcpClient();
        }

        public NetworkHandler(NodeSettings nodeSettings, TcpClient tcpClient)
            : this(nodeSettings)
        {
            this.TcpClient = tcpClient;
            this.HasInboundClient = true;
        }

        public void ConnectInboundClientAsync(IPEndPoint endPoint, CancellationToken cancellation)
        {
            this.RemoteEndPoint = endPoint;
            this.TcpClient.ConnectAsync(this.RemoteEndPoint.Address, this.RemoteEndPoint.Port).Wait(cancellation);
        }

        public async Task ConnectAsync(IPEndPoint endPoint, CancellationToken cancellation)
        {
            this.RemoteEndPoint = endPoint;

            if (this.NodeSettings.TorEnabled)
            {
                await GetTotClientAsync(this.RemoteEndPoint).ContinueWith(task =>
                {
                    TotClients.TryAdd(this.RemoteEndPoint.Address.ToString(), task.Result);
                    this.TcpClient = this.TotClient.TorSocks5Client.TcpClient;
                });
            }
            else
            {
                this.TcpClient.ConnectAsync(this.RemoteEndPoint.Address, this.RemoteEndPoint.Port).Wait(cancellation);
            }
        }

        public void Disconnect(string reason = null, Exception exception = null)
        {
            if (this.TotClient != null)
            {
                TotClients.TryRemove(this.RemoteEndPoint.Address.ToString(), out TotClient client);
                client?.DisposeAsync();
            }
            else
            {
                this.TcpClient.Close();
            }
        }

        private Task<TotClient> GetTotClientAsync(IPEndPoint endPoint)
        {
            if (this.TcpClient != null && this.TcpClient.Connected)
            {
                var connectedEndPoint = (IPEndPoint)this.TcpClient.Client.RemoteEndPoint;
                if (connectedEndPoint.Address == endPoint.Address)
                {
                    return Task.FromResult(this.TotClient);
                }
            }

            if (this.TotClient != null)
            {
                if (this.TotClient.TorSocks5Client.IsConnected)
                {
                    return Task.FromResult(this.TotClient);
                }
            }

            return this.GetNewTotClient();
        }

        private Task<TotClient> GetNewTotClient()
        {
            // change this to retrieve SOCKS port from tor config - add to NodeSettings?
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 9050);
            var socksManager = new TorSocks5Manager(serverEndPoint);

            return socksManager.EstablishTotConnectionAsync(this.RemoteEndPoint);
        }
    }
}
