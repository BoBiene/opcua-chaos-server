using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Server;

namespace opcua.chaos.server
{
    public class ChaosServerApplication
    {
        private readonly ApplicationInstance _application;
        private readonly ChaosServer _server;
        private readonly ILogger<ChaosServerApplication> _logger;

        public ChaosServerApplication(ChaosServer server, ILogger<ChaosServerApplication> logger)
        {
            _logger = logger;
            _application = new ApplicationInstance
            {
                ApplicationName = "OpcUaChaosServer",
                ApplicationType = ApplicationType.Server,
                ConfigSectionName = "OpcUaChaosServer"
            };

            _server = server;
        }

        public async Task StartAsync()
        {
            _application.ApplicationConfiguration = CreateDefaultConfiguration();

            await _application.CheckApplicationInstanceCertificate(false, 2048);
            await _application.Start(_server);
            _server.StartChaos();

            _logger.LogInformation("Chaos OPC UA Server läuft auf opc.tcp://localhost:4840/");
        }

        public async Task StopAsync() => await _server.StopAsync();

        private static ApplicationConfiguration CreateDefaultConfiguration()
        {
            var config = new ApplicationConfiguration
            {
                ApplicationName = "OpcUaChaosServer",
                ApplicationUri = Utils.Format("urn:{0}:OpcUaChaosServer", System.Net.Dns.GetHostName()),
                ApplicationType = ApplicationType.Server,
                ServerConfiguration = new ServerConfiguration
                {
                    BaseAddresses = { "opc.tcp://0.0.0.0:4840" },
                    SecurityPolicies = { new ServerSecurityPolicy { SecurityMode = MessageSecurityMode.None, SecurityPolicyUri = SecurityPolicies.None } },
                    DiagnosticsEnabled = true
                },
                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate = new CertificateIdentifier
                    {
                        StoreType = "Directory",
                        StorePath = "./pki/own",
                        SubjectName = "CN=OpcUaChaosServer, O=MaliciousTest"
                    },
                    TrustedIssuerCertificates = new CertificateTrustList { StoreType = "Directory", StorePath = "./pki/issuers" },
                    TrustedPeerCertificates = new CertificateTrustList { StoreType = "Directory", StorePath = "./pki/trusted" },
                    RejectedCertificateStore = new CertificateTrustList { StoreType = "Directory", StorePath = "./pki/rejected" },
                    AutoAcceptUntrustedCertificates = true,
                    AddAppCertToTrustedStore = false
                },
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                DisableHiResClock = false,
                TraceConfiguration = new TraceConfiguration()
            };

            config.Validate(ApplicationType.Server).GetAwaiter().GetResult();
            return config;
        }
    }
}
