using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public static class EventContract
    {
        public static void ValidateFireEvent(IEnumerator<IGraphChangedListener> eventListeners)
        {
            if (eventListeners == null)
                throw new ArgumentNullException(nameof(eventListeners));
        }
    }
}