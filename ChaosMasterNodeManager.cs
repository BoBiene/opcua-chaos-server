using Opc.Ua;
using Opc.Ua.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opcua.chaos.server
{
    internal class ChaosMasterNodeManager : MasterNodeManager
    {
        private readonly ChaosServer _chaosServer;
        public ChaosMasterNodeManager(ChaosServer chaosServer, IServerInternal server, ApplicationConfiguration configuration, string dynamicNamespaceUri, params INodeManager[] additionalManagers) 
            : base(server, configuration, dynamicNamespaceUri, additionalManagers)
        {
            _chaosServer = chaosServer;
        }

        public override void CreateMonitoredItems(OperationContext context, uint subscriptionId, double publishingInterval, TimestampsToReturn timestampsToReturn, IList<MonitoredItemCreateRequest> itemsToCreate, IList<ServiceResult> errors, IList<MonitoringFilterResult> filterResults, IList<IMonitoredItem> monitoredItems, bool createDurable)
        {
            var sessions = _chaosServer.GetSessionDictionary();   

            base.CreateMonitoredItems(context, subscriptionId, publishingInterval, timestampsToReturn, itemsToCreate, errors, filterResults, monitoredItems, createDurable);
        }
    }
}
