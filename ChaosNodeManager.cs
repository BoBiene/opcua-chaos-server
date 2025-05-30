﻿using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Server;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace opcua.chaos.server
{
    public class ChaosNodeManager : CustomNodeManager2
    {
        private readonly ILogger _logger;
        private readonly ChaosOptions _options;
        private readonly List<BaseDataVariableState> _dynamicVariables = new();
        private readonly Func<NodeIdDictionary<Session>> _getSessionsDict;
        private readonly Random _rng = new();
        private Timer? _updateTimer;

        public ChaosNodeManager(IServerInternal server, ApplicationConfiguration configuration, ILogger logger, ChaosOptions options, Func<NodeIdDictionary<Session>> getSessionsDict)
            : base(server, configuration, "http://malicious.opcua")
        {
            SystemContext.NodeIdFactory = this;
            _logger = logger;
            _options = options;
            _getSessionsDict = getSessionsDict;
        }

        protected override ServiceResult CreateMonitoredItem(ServerSystemContext context, NodeHandle handle, uint subscriptionId, double publishingInterval, DiagnosticsMasks diagnosticsMasks, TimestampsToReturn timestampsToReturn, MonitoredItemCreateRequest itemToCreate, bool createDurable, ref long globalIdCounter, out MonitoringFilterResult filterResult, out IMonitoredItem monitoredItem)
        {
            var sessions = _getSessionsDict();
            return base.CreateMonitoredItem(context, handle, subscriptionId, publishingInterval, diagnosticsMasks, timestampsToReturn, itemToCreate, createDurable, ref globalIdCounter, out filterResult, out monitoredItem);
        }

        public override void CreateMonitoredItems(OperationContext context, uint subscriptionId, double publishingInterval, TimestampsToReturn timestampsToReturn, IList<MonitoredItemCreateRequest> itemsToCreate, IList<ServiceResult> errors, IList<MonitoringFilterResult> filterErrors, IList<IMonitoredItem> monitoredItems, bool createDurable, ref long globalIdCounter)
        {
            var sessions = _getSessionsDict();
            base.CreateMonitoredItems(context, subscriptionId, publishingInterval, timestampsToReturn, itemsToCreate, errors, filterErrors, monitoredItems, createDurable, ref globalIdCounter);
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
            

            var modeNode = AddModeNode(folder);
            folder.AddChild(modeNode);

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

            AddPredefinedNode(SystemContext, folder);
            return nodes;
        }

        private BaseDataVariableState AddModeNode(FolderState folder)
        {
            string name = "ChaosMode";
            return new BaseDataVariableState(folder)
            {
                NodeId = new NodeId(name, NamespaceIndex),
                BrowseName = new QualifiedName(name, NamespaceIndex),
                DisplayName = name,
                DataType = DataTypeIds.UInt16,
                ValueRank = ValueRanks.Scalar,
                AccessLevel = AccessLevels.CurrentRead | AccessLevels.CurrentWrite,
                UserAccessLevel = AccessLevels.CurrentRead | AccessLevels.CurrentWrite,
                Value = (ushort)_options.Mode,
                OnSimpleWriteValue = OnWriteModeNode
            };
        }

        private ServiceResult OnWriteModeNode(ISystemContext context, NodeState node, ref object value)
        {
            ushort mode;

            try
            {
                mode = Convert.ToUInt16(value);
            }
            catch (InvalidCastException)
            {
                return ServiceResult.Create(StatusCodes.BadTypeMismatch, translation: null);
            }
            catch (OverflowException)
            {
                return ServiceResult.Create(StatusCodes.BadTypeMismatch, translation: null);
            }
            catch (FormatException)
            {
                return ServiceResult.Create(StatusCodes.BadTypeMismatch, translation: null);
            }
            catch
            {
                return ServiceResult.Create(StatusCodes.Bad, translation: null);
            }

            _options.Mode = (ChaosMode)mode;
            _logger.LogInformation("[CHAOS] Chaos-Modus geändert: {Mode}", mode);
            return ServiceResult.Good;
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
