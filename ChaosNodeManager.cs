using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Server;
using System;
using System.Collections.Generic;

namespace opcua.chaos.server
{
    public class ChaosNodeManager : CustomNodeManager2
    {
        private readonly ILogger _logger;
        private readonly ChaosOptions _options;
        private readonly List<BaseDataVariableState> _dynamicVariables = new();
        private readonly Random _rng = new();
        private Timer? _updateTimer;

        public ChaosNodeManager(IServerInternal server, ApplicationConfiguration configuration, ILogger logger, ChaosOptions options)
            : base(server, configuration, "http://malicious.opcua")
        {
            SystemContext.NodeIdFactory = this;
            _logger = logger;
            _options = options;
        }

        protected override NodeStateCollection LoadPredefinedNodes(ISystemContext context)
        {
            var nodes = new NodeStateCollection();

            var folder = new FolderState(null)
            {
                SymbolicName = "Chaos",
                ReferenceTypeId = ReferenceTypeIds.Organizes,
                TypeDefinitionId = ObjectTypeIds.FolderType,
                NodeId = new NodeId("Chaos", NamespaceIndex),
                BrowseName = new QualifiedName("Chaos", NamespaceIndex),
                DisplayName = new LocalizedText("Chaos"),
                EventNotifier = EventNotifiers.None
            };

            folder.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ObjectsFolder);
            AddPredefinedNode(SystemContext, folder);

            for (int i = 0; i < _options.StaticItems; i++)
            {
                var variable = CreateVariable(folder, i, isDynamic: false);
                folder.AddChild(variable);
            }

            for (int i = 0; i < _options.DynamicItems; i++)
            {
                var variable = CreateVariable(folder, i, isDynamic: true);
                folder.AddChild(variable);
                _dynamicVariables.Add(variable);
            }

            if (_dynamicVariables.Count > 0)
            {
                _updateTimer = new Timer(_ =>
                {
                    foreach (var variable in _dynamicVariables)
                    {
                        variable.Value = _rng.NextDouble() * 100;
                        variable.Timestamp = DateTime.UtcNow;
                        variable.ClearChangeMasks(SystemContext, false);
                    }
                }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            }

            nodes.Add(folder);
            return nodes;
        }

        private BaseDataVariableState CreateVariable(NodeState parent, int index, bool isDynamic)
        {
            var name = isDynamic ? $"Dynamic{index}" : $"Static{index}";
            return new BaseDataVariableState(parent)
            {
                NodeId = new NodeId(name, NamespaceIndex),
                BrowseName = new QualifiedName(name, NamespaceIndex),
                DisplayName = name,
                DataType = DataTypeIds.Double,
                ValueRank = ValueRanks.Scalar,
                AccessLevel = AccessLevels.CurrentRead | AccessLevels.CurrentWrite,
                UserAccessLevel = AccessLevels.CurrentRead | AccessLevels.CurrentWrite,
                Value = _rng.NextDouble() * 100
            };
        }
    }
}
