using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class TutorialStageModel : BaseModel
    {
        public TutorialStageModel()
        {
        }

        public List<StageInfo> TutorialStageInfos()
        {
            var list = new List<StageInfo>();
            var stageDates = DataSystem.Stages.FindAll(a => a.Id > 1000);
            foreach (var stageDate in stageDates)
            {
                var info = new StageInfo(stageDate.Id);
                list.Add(info);
            }
            return list;
        }

        public void StartTutorial(int stageId)
        {
            InitSaveStageInfo();
            MakeStageInfo(stageId,true);
        }
    }
}