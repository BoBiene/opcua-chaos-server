using Opc.Ua.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opcua.chaos.server
{
    internal class ChaosSubscription : Subscription
    {
        private readonly ChaosServer _chaosServer;
        public ChaosSubscription(ChaosServer chaosServer, IServerInternal server, Session session, uint subscriptionId, double publishingInterval, uint maxLifetimeCount, uint maxKeepAliveCount, uint maxNotificationsPerPublish, byte priority, bool publishingEnabled, uint maxMessageCount) : base(server, session, subscriptionId, publishingInterval, maxLifetimeCount, maxKeepAliveCount, maxNotificationsPerPublish, priority, publishingEnabled, maxMessageCount)
        {
            _chaosServer = chaosServer;
        }

        
    }
}
