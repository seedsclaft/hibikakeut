using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ariadne
{
    /// <Summary>
    /// Definition of arguments which is used in events.
    /// </Summary>
    [Serializable]
    public class EventArgs
    {
        public string argName;
        public int argTypeId;
        public bool isList;

        public List<string> argListValue;
        public List<UnityEngine.Object> argObjListValue;
    }
}