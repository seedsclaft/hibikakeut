using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ariadne
{
    /// <Summary>
    /// Each event data which will be processed in event parts.
    /// </Summary>
    [Serializable]
    public class EventProcessData
    {
        public int eventCategoryId;
        public List<EventArgs> eventArgs;
    }
}