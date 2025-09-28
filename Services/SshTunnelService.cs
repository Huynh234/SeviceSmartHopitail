using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Renci.SshNet;

namespace SeviceSmartHopitail.Services
{
    public class SshTunnelHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<SshTunnelHostedService> _logger;
        private readonly IConfiguration _config;

        private SshClient? _sshClient;
        private ForwardedPortRemote? _forwardedPort;

        public SshTunnelHostedService(ILogger<SshTunnelHostedService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var sshConfig = _config.GetSection("SshTunnel");
                var host = sshConfig["Host"] ?? "your-ssh-server.com";
                var user = sshConfig["Username"] ?? "user";
                var password = sshConfig["Password"] ?? "password";
                var remoteBindAddress = sshConfig["RemoteBindAddress"] ?? "0.0.0.0";
                var remotePort = uint.Parse(sshConfig["RemotePort"] ?? "2222");
                var localHost = sshConfig["LocalHost"] ?? "127.0.0.1";
                var localPort = uint.Parse(sshConfig["LocalPort"] ?? "5000");

                _sshClient = new SshClient(host, user, password);
                _sshClient.Connect();

                _forwardedPort = new ForwardedPortRemote(remoteBindAddress, remotePort, localHost, localPort);
                _sshClient.AddForwardedPort(_forwardedPort);
                _forwardedPort.Start();

                _logger.LogInformation(
                    "Reverse SSH tunnel started: {Host}:{RemotePort} -> {LocalHost}:{LocalPort}",
                    host, remotePort, localHost, localPort
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start SSH tunnel.");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_forwardedPort != null && _forwardedPort.IsStarted)
                {
                    _forwardedPort.Stop();
                    _logger.LogInformation("SSH tunnel stopped.");
                }

                if (_sshClient != null && _sshClient.IsConnected)
                {
                    _sshClient.Disconnect();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while stopping SSH tunnel.");
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _forwardedPort?.Dispose();
            _sshClient?.Dispose();
        }
    }
}
