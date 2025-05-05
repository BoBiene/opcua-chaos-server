using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opcua.chaos.server
{
    public class ChaosOptions
    {
        public ChaosMode Mode { get; set; } = ChaosMode.Default;
        public int IntervalSeconds { get; set; } = 5;
        public double Probability { get; set; } = 0.5;
        public int StaticItems { get; set; } = 2;
        public int DynamicItems { get; set; } = 3;
    }
}
