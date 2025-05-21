using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ryneus
{
    public partial class BaseModel
    {
        public void MakeStageInfo(int stageId,bool newGame,int clearCount = 0)
        {
            var stageInfo = new StageInfo(stageId);
            // アイテムを獲得
            foreach (var getItemInfo in StageOpeningGetItemInfos(stageId,clearCount))
            {
                AddGetItemInfo(getItemInfo);
            }

            // 初期編成を作成
            var initUnitInfo = new UnitInfo();
            initUnitInfo.Index.SetValue(1);
            var actorInfo = PartyInfo.ActorInfos[0];
            var battlerInfo = new BattlerInfo(actorInfo,1);
            var battlerInfo2 = new BattlerInfo();
            initUnitInfo.SetBattlers(new List<BattlerInfo>(){battlerInfo,battlerInfo2});
            PartyInfo.AddUnitInfos(initUnitInfo);
            /*
            var unitInfos = GetStageHexUnitInfos(stageId,clearCount);
            // Fieldをセット
            stageInfo.SetHexUnitInfos(unitInfos.FindAll(a => !a.IsUnit));
            if (newGame)
            {
                // 味方チームを作成
                var mainTeam = new TeamInfo();
                mainTeam.TeamId.SetValue((int)TeamIdType.Home);

                // 初期編成を作成
                var depaterUnitInfo = new UnitInfo();
                depaterUnitInfo.Index.SetValue(1);
                var actorInfo = PartyInfo.ActorInfos[0];
                var battlerInfo = new BattlerInfo(actorInfo,1);
                depaterUnitInfo.SetBattlers(new List<BattlerInfo>(){battlerInfo,new BattlerInfo()});
                mainTeam.SetDepatuerInfos(new List<UnitInfo>(){depaterUnitInfo});

                // 拠点数
                var actPoint = unitInfos.FindAll(a => a.IsBasementUnit && a.IsFriend(mainTeam.TeamId.Value));
                mainTeam.SetActPoint(actPoint.Count);
                stageInfo.AddTeamInfo(mainTeam);
            } else
            {
                if (CurrentStage != null)
                {
                    // Fieldを引継ぎ
                    foreach (var fieldHex in CurrentStage.FieldHexList)
                    {
                        if (fieldHex.HexUnitType == HexUnitType.None)
                        {
                            continue;
                        }
                        stageInfo.AddHexUnitInfo(fieldHex);
                    }
                    var mainTeam = CurrentStage.TeamInfos.Find(a => a.TeamId.Value == (int)TeamIdType.Home);
                    // 拠点数
                    var actPoint = CurrentStage.AllFieldUnitInfos().FindAll(a => a.IsBasementUnit && a.IsFriend(mainTeam.TeamId.Value));
                    mainTeam.SetActPoint(actPoint.Count);
                    stageInfo.AddTeamInfo(mainTeam);
                }
            }

            TeamInfo awayTeam = null;
            if (newGame)
            {
                awayTeam = new TeamInfo();
                awayTeam.TeamId.SetValue((int)TeamIdType.Away);
            } else
            {
                if (CurrentStage != null)
                {
                    awayTeam = CurrentStage.TeamInfos.Find(a => a.TeamId.Value == (int)TeamIdType.Away);
                }
            }
            awayTeam.ClearMoveEndUnitIds();
            awayTeam.RemoveLostUnitInfos();
            stageInfo.AddTeamInfo(awayTeam);

            foreach (var unitInfo in unitInfos)
            {
                if (unitInfo.IsBattlerUnit)
                {
                    awayTeam.AddUnitInfos(unitInfo);
                }
            }

            // 敵は敵部隊数分1回ずつ行動可能
            var awayActPoint = unitInfos.FindAll(a => a.IsUnit && a.IsFriend(awayTeam.TeamId.Value));
            awayTeam.SetActPoint(awayActPoint.Count - 1);
            */
            CurrentGameInfo.SetStageInfo(stageInfo);
            PartyInfo.StageId.SetValue(stageId);
        }

        public List<GetItemInfo> StageOpeningGetItemInfos(int stageId,int clearCount)
        {
            var getItemInfos = new List<GetItemInfo>();
            var stageSymbolDates = DataSystem.FindStageSymbolData(stageId);
            if (stageSymbolDates == null)
            {
                return getItemInfos;
            }
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
            var stageSymbols = DataSystem.FindStageSymbolData(stageId);
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
