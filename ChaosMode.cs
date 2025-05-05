using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opcua.chaos.server
{
    public enum ChaosMode
    {
        /// <summary>
        /// No chaos is applied.
        /// </summary>
        None,

        /// <summary>
        /// Clears monitored items and breaks the publish engine (combined effects).
        /// </summary>
        Default,

        /// <summary>
        /// Clears all monitored items in a subscription.
        /// </summary>
        ClearItems,

        /// <summary>
        /// Sets the subscription's publish engine to null.
        /// </summary>
        BreakEngine,

        /// <summary>
        /// Removes the subscription from the session completely.
        /// </summary>
        RemoveSubscription
    }
}
