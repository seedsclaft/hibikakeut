using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class StageInfo
    {
        public StageData Master => DataSystem.FindStage(StageId.Value);
        public ParameterInt StageId = new();
        public StageInfo(int id)
        {
            StageId.SetValue(id);
        }

        public bool CheckVictory()
        {
            var achieveType = Master.AchieveType;
            return false;
        }

        public bool CheckGameOver()
        {
            return false;
        }
    }
}