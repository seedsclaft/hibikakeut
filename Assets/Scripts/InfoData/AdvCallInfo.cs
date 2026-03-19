using System;

namespace Ryneus
{
    public class AdvCallInfo
    {
        public AdvCallInfo(string label)
        {
            Label.SetValue(label);
        }
        public ParameterString Label = new();
        public ParameterAction CallEvent = new();
        public void SetCallEvent(Action callEvent)
        {
            CallEvent.SetValue(callEvent);
        }
    }
}
