using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Opc.Ua;
using Opc.Ua.Server;
using Org.BouncyCastle.Pkcs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace opcua.chaos.server
{
    internal class ChaosSessionManager : SessionManager
    {
        private readonly int m_maxRequestAge;
        private readonly int m_maxBrowseContinuationPoints;
        private readonly int m_maxHistoryContinuationPoints;
        private readonly ChaosServer _chaosServer;

        public ChaosSessionManager(ChaosServer chaosServer,IServerInternal server, ApplicationConfiguration configuration) : base(server, configuration)
        {
            _chaosServer = chaosServer;
            m_maxRequestAge = configuration.ServerConfiguration.MaxRequestAge;
            m_maxBrowseContinuationPoints = configuration.ServerConfiguration.MaxBrowseContinuationPoints;
            m_maxHistoryContinuationPoints = configuration.ServerConfiguration.MaxHistoryContinuationPoints;
        }

        protected override Session CreateSession(OperationContext context,
            IServerInternal server,
            X509Certificate2 serverCertificate,
            NodeId sessionCookie,
            byte[] clientNonce,
            Nonce serverNonce,
            string sessionName,
            ApplicationDescription clientDescription,
            string endpointUrl,
            X509Certificate2 clientCertificate,
            X509Certificate2Collection clientCertificateChain,
            double sessionTimeout,
            uint maxResponseMessageSize,
            int maxRequestAge,
            int maxContinuationPoints)
        {
            Session session = new ChaosSession(
                _chaosServer,
                context,
                server,
                serverCertificate,
                sessionCookie,
                clientNonce,
                serverNonce,
                sessionName,
                clientDescription,
                endpointUrl,
                clientCertificate,
                clientCertificateChain,
                sessionTimeout,
                maxResponseMessageSize,
                m_maxRequestAge,
                m_maxBrowseContinuationPoints,
                m_maxHistoryContinuationPoints);

            return session;
        }

    }
}
