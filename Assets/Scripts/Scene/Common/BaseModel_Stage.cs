using System.Collections.Generic;
using System.Diagnostics;

namespace Ryneus
{
    public partial class BaseModel
    {
        public void MakeStageInfo(int stageId,int clearCount = 0)
        {
            var stageInfo = new StageInfo(stageId);
            // アイテムを獲得
            foreach (var getItemInfo in StageOpeningGetItemInfos(stageId,clearCount))
            {
                AddGetItemInfo(getItemInfo);          
            }
            var unitInfos = GetStageHexUnitInfos(stageId,clearCount);
            stageInfo.SetHexUnitInfos(unitInfos);
            // 味方チームを作成
            var mainTeam = new TeamInfo();
            mainTeam.TeamId.SetValue((int)TeamIdType.Home);
            // 拠点数
            var actPoint = unitInfos.FindAll(a => a.IsBasementUnit() && a.IsFriend(mainTeam.TeamId.Value));
            mainTeam.ActPoint.SetValue(actPoint.Count);
            mainTeam.CurrentActPoint.SetValue(actPoint.Count);
            stageInfo.AddTeamInfo(mainTeam);

            // 存在する陣営を作成
            var awayTeam = new TeamInfo();
            awayTeam.TeamId.SetValue((int)TeamIdType.Away);
            foreach (var unitInfo in unitInfos)
            {
                if (unitInfo.HexUnitType == HexUnitType.Battler)
                {
                    awayTeam.AddUnitInfos(unitInfo);
                }
            }
            var awayActPoint = unitInfos.FindAll(a => a.IsBasementUnit() && a.IsFriend(awayTeam.TeamId.Value));
            awayTeam.ActPoint.SetValue(awayActPoint.Count);
            awayTeam.CurrentActPoint.SetValue(awayActPoint.Count);
            stageInfo.AddTeamInfo(awayTeam);
            
            
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
            var stageData = DataSystem.FindStage(stageId);
            var stageSymbols = stageData.StageSymbols;
            return StageHexUnitInfos(stageSymbols,clearCount);
        }




        public void UpdateUnitStatus()
        {
            foreach (var actorInfo in PartyInfo.ActorInfos)
            {
                actorInfo.ChangeHp(actorInfo.MaxHp);
            }
        }
    }
}
