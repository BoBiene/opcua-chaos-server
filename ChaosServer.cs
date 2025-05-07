using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opc.Ua;
using Opc.Ua.Server;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opcua.chaos.server
{
    public class ChaosServer : StandardServer
    {
        private Timer? _chaosTimer;
        private readonly Random _rng = new();
        private readonly ILogger<ChaosServer> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ChaosOptions _options;

        public ChaosServer(ILogger<ChaosServer> logger, ILoggerFactory loggerFactory, IOptions<ChaosOptions> options)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _options = options.Value;
        }

        protected override SubscriptionManager CreateSubscriptionManager(IServerInternal server, ApplicationConfiguration configuration)
        {
            return new ChaosSubscriptionManager(this, server, configuration, _loggerFactory.CreateLogger<ChaosSubscriptionManager>());
        }


        protected override MasterNodeManager CreateMasterNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        {
            _logger.LogInformation("[INIT] Erstelle NodeManager...");

            var nodeManagers = new List<INodeManager> {
                new ChaosNodeManager(server, configuration, _loggerFactory.CreateLogger<ChaosNodeManager>(), _options,GetSessionDictionary)
            };
            return new ChaosMasterNodeManager(this, server, configuration, null, nodeManagers.ToArray());
        }

        protected override SessionManager CreateSessionManager(IServerInternal server, ApplicationConfiguration configuration)
        {
            return new ChaosSessionManager(this, server, configuration);
        }

        public NodeIdDictionary<Session> GetSessionDictionary()
            => ReflectionHelper.GetPrivateMember<NodeIdDictionary<Session>>(ServerInternal.SessionManager, "m_sessions");
        public bool DestroySession(Session session)
        {
            var sessions = GetSessionDictionary();

            foreach (var kvp in sessions)
            {
                if (kvp.Value == session)
                {
                    sessions.Remove(kvp.Key);
                    return true;
                }
            }
            return false;
        }


        public void StartChaos()
        {
            if (_options.Mode == ChaosMode.None)
            {
                _logger.LogInformation("[CHAOS] Chaos-Modus ist deaktiviert.");
                return;
            }

            _chaosTimer = new Timer(_ =>
            {
                var sessions = ReflectionHelper.GetPrivateMember<NodeIdDictionary<Session>>(ServerInternal.SessionManager, "m_sessions");

                if (sessions == null || sessions.Count == 0)
                {
                    _logger.LogInformation("[CHAOS] Keine Sessions aktiv.");
                    return;
                }

                _logger.LogInformation("[CHAOS] Aktive Sessions: {Count}", sessions.Count);

                foreach (var keyValuePair in sessions)
                {
                    var session = keyValuePair.Value;
                    if (_options.Mode == ChaosMode.CloseSession)
                    {
                        if (_rng.NextDouble() >= _options.Probability) continue;
                        session.Dispose();

                        sessions.Remove(keyValuePair.Key);

                        _logger.LogWarning("[CHAOS][{Mode}] Session {SessionId} removed.",
                                            _options.Mode, session.Id);
                    }
                    else
                    {
                        var subs = ServerInternal.SubscriptionManager.GetSubscriptions()
                            .Where(s => s.Session == session).ToList();
                        if (subs.Count == 0) continue;

                        foreach (var target in subs)
                        {
                            if (_rng.NextDouble() >= _options.Probability) continue;

                            var context = new OperationContext(new RequestHeader(), RequestType.Unknown, session);

                            switch (_options.Mode)
                            {

                                case ChaosMode.ClearItems:
                                    target.GetMonitoredItems(out var serverHandles, out var _);
                                    if (serverHandles.Length > 0)
                                    {
                                        target.DeleteMonitoredItems(context, new UInt32Collection(serverHandles), out var _, out var _);
                                        target.SessionClosed();
                                        _logger.LogWarning("[CHAOS][{Mode}] {Count} MonitoredItems in Subscription {Id} gelöscht.",
                                            _options.Mode, serverHandles.Length, target.Id);
                                    }
                                    break;

                                case ChaosMode.RemoveSubscription:
                                    ServerInternal.SubscriptionManager.DeleteSubscription(context, target.Id);
                                    _logger.LogWarning("[CHAOS][RemoveSubscription] Subscription {Id} gelöscht.", target.Id);
                                    break;

                                case ChaosMode.BreakEngine:
                                    target.SetPublishingMode(context, false);
                                    _logger.LogWarning("[CHAOS][BreakEngine] Publishing für Subscription {Id} deaktiviert.", target.Id);
                                    break;
                            }
                        }
                    }
                }
            }, null, TimeSpan.FromSeconds(_options.IntervalSeconds), TimeSpan.FromSeconds(_options.IntervalSeconds));
        }

        protected override void ValidateRequest(RequestHeader requestHeader)
        {
            base.ValidateRequest(requestHeader);
        }

        protected override OperationContext ValidateRequest(RequestHeader requestHeader, RequestType requestType)
        {
            if(requestType == RequestType.CreateMonitoredItems)
            {
                var session = GetSessionDictionary().FirstOrDefault(s => s.Value.Id == requestHeader.AuthenticationToken);
                if (session.Value != null)
                {
                    _logger.LogWarning("[CHAOS] Session {SessionId} versucht, MonitoredItems zu erstellen.", session.Key);
                    DestroySession(session.Value);
                }
            }
            return base.ValidateRequest(requestHeader, requestType);
        }


        public Task StopAsync()
        {
            _chaosTimer?.Dispose();
            return Task.CompletedTask;
        }
    }
}
