﻿using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    ///     An event generated by changes to the graph.
    /// </summary>
    public interface IEvent
    {
        void FireEvent(IEnumerator<IGraphChangedListener> eventListeners);
    }
}