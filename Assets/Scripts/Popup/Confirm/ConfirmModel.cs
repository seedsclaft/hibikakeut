using System.Collections;
using System.Collections.Generic;
using System;

namespace Ryneus
{
    public class ConfirmModel : BaseModel
    {
        public List<SystemData.CommandData> ConfirmCommand(List<int> textIds)
        {
            if (textIds.Count == 0)
            {
                return BaseConfirmCommand(3050, 3051);
            }
            return BaseConfirmCommand(textIds[0], textIds[1]);
        }

        public List<SystemData.CommandData> NoChoiceConfirmCommand(List<int> textIds)
        {
            if (textIds.Count == 0)
            {
                return new List<SystemData.CommandData>() { BaseConfirmCommand(3052, 0)[0] };
            }
            return BaseConfirmCommand(textIds[0]);
        }
    }
}
