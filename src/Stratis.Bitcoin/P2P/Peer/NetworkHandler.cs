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
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

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
        private Process TorProcess;

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

        public bool InitialiseTor()
        {
            var torProcess = Process.GetProcessesByName("tor");
            if (torProcess.Length == 0)
            {
                this.TorProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "tor",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    },
                    EnableRaisingEvents = true
                };

                // this may not trigger in all cases - https://github.com/dotnet/coreclr/issues/8565
                AppDomain.CurrentDomain.ProcessExit += NetworkHandler_Exited;

                this.NodeSettings.Logger.LogInformation("Starting Tor");

                this.TorProcess.Start();
                var result = "";
                var finishedInitialising = false;
                do
                {
                    result = this.TorProcess.StandardOutput.ReadLine();
                    finishedInitialising = result.Contains("Bootstrapped 100%: Done", StringComparison.InvariantCultureIgnoreCase);
                }
                while (!finishedInitialising && !this.TorProcess.StandardOutput.EndOfStream);
                return finishedInitialising;
            }
            else
            {
                return true;
            }
        }

        private void NetworkHandler_Exited(object sender, EventArgs e)
        {
            this.TorProcess.Close();
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
