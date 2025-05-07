using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opcua.chaos.server
{
    internal class ChaosSubscriptionManager : SubscriptionManager
    {
        private readonly ChaosServer _chaosServer;
        private readonly IServerInternal _server;
        private readonly ILogger<ChaosSubscriptionManager> _logger;
        private readonly uint m_maxMessageCount;
        public ChaosSubscriptionManager(ChaosServer chaosServer, IServerInternal server, ApplicationConfiguration configuration, ILogger<ChaosSubscriptionManager> logger ) 
            : base(server, configuration)
        {
            _chaosServer = chaosServer;
            _server = server;
            _logger = logger;
            m_maxMessageCount = (uint)configuration.ServerConfiguration.MaxMessageQueueSize;
        }

      

        protected override Subscription CreateSubscription(OperationContext context, uint subscriptionId, double publishingInterval, uint lifetimeCount, uint keepAliveCount, uint maxNotificationsPerPublish, byte priority, bool publishingEnabled)
        {
            Subscription subscription = new ChaosSubscription(
                _chaosServer,
               _server,
               context.Session,
               subscriptionId,
               publishingInterval,
               lifetimeCount,
               keepAliveCount,
               maxNotificationsPerPublish,
               priority,
               publishingEnabled,
               m_maxMessageCount);

            return subscription;
        }
     
    }
}
