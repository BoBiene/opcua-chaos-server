using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opcua.chaos.server
{
    public class ChaosServerHostedService : IHostedService
    {
        private readonly ChaosServerApplication _app;

        public ChaosServerHostedService(ChaosServerApplication app)
        {
            _app = app;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _app.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _app.StopAsync();
        }
    }
}
