﻿using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class MainMenuModel : BaseModel
    {
        public bool IsEnding()
        {
            return false;//PartyInfo.HasEndingGetItem();
        }

        public void StartSelectStage(int stageId)
        {
            if (SelectedStage(stageId))
            {
                PartyInfo.SetSeek(SelectedStageCurrentTurn(stageId));
            } else
            {
                // 新規レコード作成
                foreach (var record in GetStageSymbolInfos(stageId))
                {
                    //PartyInfo.SetSymbolResultInfo(record);
                }
            }
            SavePlayerStageData(true);
        }
        
        private bool SelectedStage(int stageId)
        {
            return false;
            //var records = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == stageId);
            //return records.Count > 0;
        }

        public int SelectedStageCurrentTurn(int stageId)
        {
            var turn = 0;
            /*
            var records = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == stageId && a.Selected);
            foreach (var record in records)
            {
                if (record.Seek >= turn)
                {
                    turn = record.Seek;
                }
            }
            */
            return turn + 1;
        }

        public StageInfo NextStage()
        {
            var list = new List<StageInfo>();
            var find = DataSystem.Stages.Find(a => a.Id > CurrentStage.Id);
            if (find != null)
            {
                return new StageInfo(find.Id);
            }
            return null;
        }
    }
}