using Opc.Ua;
using Opc.Ua.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace opcua.chaos.server
{
    internal class ChaosSession : Opc.Ua.Server.Session
    {
        private readonly ChaosServer _chaosServer;
        public ChaosSession(ChaosServer chaosServer,
            OperationContext context,
            IServerInternal server,
            X509Certificate2 serverCertificate,
            NodeId authenticationToken,
            byte[] clientNonce,
            Nonce serverNonce,
            string sessionName,
            ApplicationDescription clientDescription,
            string endpointUrl,
            X509Certificate2 clientCertificate,
            X509Certificate2Collection clientCertificateChain,
            double sessionTimeout,
            uint maxResponseMessageSize,
            double maxRequestAge,
            int maxBrowseContinuationPoints,
            int maxHistoryContinuationPoints) : base(
                context,
                server,
                serverCertificate,
                authenticationToken,
                clientNonce,
                serverNonce,
                sessionName,
                clientDescription,
                endpointUrl,
                clientCertificate,
                clientCertificateChain,
                sessionTimeout,
                maxResponseMessageSize,
                maxRequestAge,
                maxBrowseContinuationPoints,
                maxHistoryContinuationPoints)
        {
            _chaosServer = chaosServer;
        }


        public override void ValidateRequest(RequestHeader requestHeader, RequestType requestType)
        {
            if (requestType == RequestType.CreateMonitoredItems)
            {
                _chaosServer.DestroySession(this);
            }

            base.ValidateRequest(requestHeader, requestType);
        }


    }
}
