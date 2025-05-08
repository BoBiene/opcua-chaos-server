using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opcua.chaos.server
{
    public class ChaosOptions
    {
        public ChaosMode Mode { get; set; } = ChaosMode.RemoveSubscription;
        public int IntervalSeconds { get; set; } = 20;
        public double Probability { get; set; } = 0.4;
        public int StaticItems { get; set; } = 2;
        public int DynamicItems { get; set; } = 3;
    }
}
