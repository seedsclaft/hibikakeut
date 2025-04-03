using System.Collections.Generic;
using System.Diagnostics;

namespace Ryneus
{
    public partial class BaseModel
    {
        public void MakeStageInfo(int stageId,int clearCount = 0)
        {
            var stageInfo = new StageInfo(stageId);
            foreach (var getItemInfo in StageOpeningGetItemInfos(stageId,clearCount))
            {
                AddGetItemInfo(getItemInfo);          
            }
            stageInfo.SetHexUnitInfos(GetStageHexUnitInfos(stageId,clearCount));
            CurrentGameInfo.SetStageInfo(stageInfo);
            PartyInfo.StageId.SetValue(stageId);
            PartyInfo.StartStage.SetValue(false);
        }
        
        public List<GetItemInfo> StageOpeningGetItemInfos(int stageId,int clearCount)
        {
            var getItemInfos = new List<GetItemInfo>();
            var stageSymbolDates = DataSystem.FindStage(stageId).StageSymbols;
            stageSymbolDates = stageSymbolDates.FindAll(a => a.InitX == -1 && a.ClearCount <= clearCount);
            foreach (var stageSymbolData in stageSymbolDates)
            {
                if (stageSymbolData.PrizeSetId != 0)
                {
                    var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == stageSymbolData.PrizeSetId);
                    foreach (var prizeSet in prizeSets)
                    {
                        var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                        AddGetItemInfo(getItemInfo);
                    }
                }
            }
            return getItemInfos;
        }

        public List<HexUnitInfo> GetStageHexUnitInfos(int stageId,int clearCount)
        {
            return StageHexUnitInfos(DataSystem.FindStage(stageId).StageSymbols,clearCount);
        }



        public void SeekNext()
        {
            foreach (var actorInfo in PartyInfo.ActorInfos)
            {
                actorInfo.ChangeHp(actorInfo.MaxHp);
            }
        }
    }
}
