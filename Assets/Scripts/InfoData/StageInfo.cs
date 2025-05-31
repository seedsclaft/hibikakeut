using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class StageInfo
    {
        public StageData Master => DataSystem.FindStage(StageId.Value);
        public ParameterInt StageId = new();
        public ParameterBool Cleared = new();
        public StageInfo(int id,bool cleared = false)
        {
            StageId.SetValue(id);
            Cleared.SetValue(cleared);
        }

        public bool CheckVictory()
        {
            return false;
        }

        public bool CheckGameOver()
        {
            return false;
        }
    }
}