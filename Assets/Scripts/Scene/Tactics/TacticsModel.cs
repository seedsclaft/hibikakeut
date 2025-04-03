using System.Collections;
using System.Collections.Generic;
using System;

namespace Ryneus
{
    public class TacticsModel : BaseModel
    {
        private TacticsSceneInfo _sceneParam;
        public TacticsSceneInfo SceneParam => _sceneParam;
        public TacticsModel()
        {
            _sceneParam = (TacticsSceneInfo)GameSystem.SceneStackManager.LastSceneParam;
            SetFirstBattleActorId();
            var stageData = CurrentGameInfo.StageInfo?.Master;
            if (stageData != null)
            {
                _hexRoute = new HexRoute(stageData.Width,stageData.Height,CurrentGameInfo.StageInfo.HexUnitList);
            }
        }
        
        private ActorInfo _swapFromActor = null;
        public ActorInfo SwapFromActor => _swapFromActor;
        public void SetSwapFromActorInfo(ActorInfo actorInfo) => _swapFromActor = actorInfo; 
        public void SwapActorInfo(ActorInfo actorInfo)
        {
            if (_swapFromActor == null)
            {
                return;
            }
            if (_swapFromActor == actorInfo)
            {
                return;
            }
            var fromIndex = _swapFromActor.BattleIndex.Value;
            var toIndex = actorInfo.BattleIndex.Value;
            _swapFromActor.BattleIndex.SetValue(toIndex);
            actorInfo.BattleIndex.SetValue(fromIndex);
        }

        public bool StageStart()
        {
            if (PartyInfo != null)
            {
                return PartyInfo.StartStage.Value == false;
            }
            return false;
        }

        public Effekseer.EffekseerEffectAsset StartStageAnimation()
        {
            return ResourceSystem.LoadResourceEffect("IceOne1");
        }


        public void SetFirstBattleActorId()
        {
            var stageMembers = StageMembers();
            if (stageMembers.Count > 0)
            {
                var firstBattler = stageMembers.Find(a => a.BattleIndex.Value == 1);
                if (firstBattler != null)
                {
                    SetSelectActorId(firstBattler.ActorId.Value);
                } else
                {
                    SetSelectActorId(stageMembers[0].ActorId.Value);
                }
            }
        }
        
        private int _selectActorId = 0;
        public void SetSelectActorId(int actorId)
        {
            _selectActorId = actorId;
        }    
        public ActorInfo TacticsActor()
        {
            return StageMembers().Find(a => a.ActorId.Value == _selectActorId);
        }

        private List<int> _shopSelectIndexes = new ();

        public List<SystemData.CommandData> TacticsCommand()
        {
            return DataSystem.TacticsCommand;
        }

        public ListData ChangeEnableCommandData(int index,bool enable)
        {
            return new ListData(DataSystem.TacticsCommand[index],index,enable);
        }

        public List<ListData> StageResultInfos(SymbolResultInfo symbolResultInfo)
        {
            /*
            var selectRecords = PartyInfo.SymbolRecordList.FindAll(a => a.IsSameStageSeek(symbolResultInfo.StageId,symbolResultInfo.Seek,symbolResultInfo.WorldType));
            selectRecords.Sort((a,b) => a.SeekIndex > b.SeekIndex ? 1 : -1);
            Func<SymbolResultInfo,bool> enable = (a) => 
            {
                var enable = false;
                if (a.StageId == CurrentStage.Id && a.Seek <= CurrentStage.Seek)
                {
                    enable = true;
                }
                if (a.StageId < CurrentStage.Id)
                {
                    enable = true;
                }
                return a.Seek == CurrentStage.Seek || enable;
            };
            var seekIndex = 0;
            if (CurrentSelectRecord() != null)
            {
                seekIndex = CurrentSelectRecord().SeekIndex;
            }
            return MakeListData(selectRecords,enable,seekIndex);
            */
            return null;
        }

        public void SetStageSeekIndex(int seekIndex)
        {
            CurrentStage.SeekIndex.SetValue(seekIndex);
        }


        public List<SkillInfo> AlcanaMagicSkillInfos(List<GetItemInfo> getItemInfos)
        {
            var skillInfos = new List<SkillInfo>();
            foreach (var getItemInfo in getItemInfos)
            {
                if (getItemInfo.GetItemType == GetItemType.Skill)
                {
                    var skillInfo = new SkillInfo(getItemInfo.Param1);
                    var cost = 0;
                    skillInfo.SetEnable(cost <= Currency);
                    skillInfos.Add(skillInfo);
                }
            }
            return skillInfos;
        }

        public void MakeSelectRelic(int skillId)
        {
            /*
            var getItemInfos = CurrentSelectRecord().SymbolInfo.GetItemInfos;
            var selectRelicInfos = getItemInfos.FindAll(a => a.GetItemType == GetItemType.Skill);
            // 魔法取得
            var selectRelic = selectRelicInfos.Find(a => a.Param1 == skillId);
            foreach (var selectRelicInfo in selectRelicInfos)
            {
                selectRelicInfo.SetGetFlag(false);
            }
            selectRelic.SetGetFlag(true);
            AddPlayerInfoSkillId(skillId);
            */
        }

        public List<SkillInfo> ShopMagicSkillInfos(List<GetItemInfo> getItemInfos)
        {
            //var alcanaIdList = PartyInfo.CurrentAlchemyIdList(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
            var skillInfos = new List<SkillInfo>();
            /*
            foreach (var getItemInfo in getItemInfos)
            {
                var skillInfo = new SkillInfo(getItemInfo.Param1);
                var cost = ShopLearningCost(skillInfo);
                skillInfo.SetEnable(cost <= (Currency - LearningShopMagicCost()) && !_shopSelectIndexes.Contains(skillInfo.Id) && !alcanaIdList.Contains(skillInfo.Id));
                skillInfo.SetLearningCost(cost);
                skillInfos.Add(skillInfo);
            }
            */
            return skillInfos;
        }

        public List<ListData> SideMenu()
        {
            var list = new List<SystemData.CommandData>();
            var retire = new SystemData.CommandData
            {
                Id = 1,
                Name = DataSystem.GetText(13410),
                Key = "Option"
            };
            list.Add(retire);
            var menuCommand = new SystemData.CommandData
            {
                Id = 2,
                Name = DataSystem.GetText(19700),
                Key = "Help"
            };
            list.Add(menuCommand);
            var dictionaryCommand = new SystemData.CommandData
            {
                Id = 11,
                Name = DataSystem.GetText(19730),
                Key = "Dictionary"
            };
            list.Add(dictionaryCommand);
            var saveCommand = new SystemData.CommandData
            {
                Id = 3,
                Name = DataSystem.GetText(19710),
                Key = "Save"
            };
            list.Add(saveCommand);
            var titleCommand = new SystemData.CommandData
            {
                Id = 4,
                Name = DataSystem.GetText(19720),
                Key = "Title"
            };
            list.Add(titleCommand);
            Func<SystemData.CommandData,bool> enable = (a) => 
            {
                if (a.Key == "Save" || a.Key == "Retire")
                {
                    //return PartyInfo.ReturnSymbol == null;
                }
                return true;
            };
            return MakeListData(list,enable);
        }


        public void ResetBattlerIndex()
        {
            foreach (var stageMember in StageMembers())
            {
                //stageMember.SetBattleIndex(-1);
            }
        }

        public bool EnableShopMagic(SkillInfo skillInfo)
        {
            var cost = ShopLearningCost(skillInfo);
            return cost <= (Currency - LearningShopMagicCost());
        }

        public void PayShopCurrency(SkillInfo skillInfo)
        {
            if (EnableShopMagic(skillInfo))
            {
                var cost = ShopLearningCost(skillInfo);
                //var getItemInfo = CurrentSelectRecord().SymbolInfo.GetItemInfos.Find(a => a.Param1 == skillInfo.Id);
                //getItemInfo?.SetResultParam(cost);
                //getItemInfo.SetGetFlag(true);
                _shopSelectIndexes.Add(skillInfo.Id.Value);
            }
        }

        public bool IsSelectedShopMagic(SkillInfo skillInfo)
        {
            return _shopSelectIndexes.Contains(skillInfo.Id.Value);
        }

        public void CancelShopCurrency(SkillInfo skillInfo)
        {
            if (EnableShopMagic(skillInfo))
            {
                //var getItemInfo = CurrentSelectRecord().SymbolInfo.GetItemInfos.Find(a => a.Param1 == skillInfo.Id);
                //getItemInfo?.SetResultParam(0);
                //getItemInfo.SetGetFlag(false);
                _shopSelectIndexes.Remove(skillInfo.Id.Value);
            }
        }

        public int ShopLearningCost(SkillInfo skillInfo)
        {
            if (skillInfo.Master.Rank == RankType.ActiveRank2 || skillInfo.Master.Rank == RankType.PassiveRank2 || skillInfo.Master.Rank == RankType.EnhanceRank2)
            {
                return 20;
            }
            return 10;
        }

        public List<GetItemInfo> LearningShopMagics()
        {
            var list = new List<GetItemInfo>();
            /*
            foreach (var getItemInfo in CurrentSelectRecord().SymbolInfo.GetItemInfos)
            {
                if (_shopSelectIndexes.Contains(getItemInfo.Param1))
                {
                    list.Add(getItemInfo);
                }
            }
            */
            return list;
        }

        public int LearningShopMagicCost()
        {
            int cost = 0;
            foreach (var getItemInfo in LearningShopMagics())
            {
                cost += getItemInfo.ResultParam;
            }
            return cost;
        }

        public ParameterInt FieldX = new();
        public ParameterInt FieldY = new();
        private HexRoute _hexRoute;
        private List<HexField> _reachPathes = new();
        private string _commandKey = "";
        public string CommandKey =>_commandKey;
        public void SetCommandKey(string key) => _commandKey = key;
        private ParameterInt SelectingHexUnitId = new();
        private int _departureActorId = -1;
        public void SetDepatureActorId(int departureActorId) => _departureActorId = departureActorId;
        public void MoveFieldXY(int x,int y)
        {
            if (_reachPathes.Count > 0)
            {
                var nextX = FieldX.Value + x;
                var nextY = FieldY.Value + y;
                if (_reachPathes.Find(a => a.X == nextX && a.Y == nextY) == null)
                {
                    return;
                }
            }
            var stageData = PartyInfo.StageMaster;
            FieldX.GainValue(x,0,stageData.Width-1);
            FieldY.GainValue(y,0,stageData.Height-1);
        }

        public List<HexField> HexTiles()
        {
            var stageData = PartyInfo.StageMaster;
            var list = new List<HexField>();
            for (int j = 0;j < stageData.Height;j++)
            {
                for (int i = 0;i < stageData.Width;i++)
                {
                    var field = new HexField
                    {
                        X = i,
                        Y = j
                    };
                    list.Add(field);
                }
            }
            return list;
        }
        
        public HexUnitInfo HexUnit()
        {
            var hexUnit = CurrentGameInfo.StageInfo.HexUnitList.FindAll(a => a.HexField.X == FieldX.Value && a.HexField.Y == FieldY.Value);
            if (hexUnit.Count > 1)
            {
                hexUnit.Sort((a,b) => a.HexUnitType > b.HexUnitType ? -1 : 1);
            }
            return hexUnit.Count > 0 ? hexUnit[0] : null;
        }

        public void MakeDepartureHex()
        {
            var hexUnit = CurrentGameInfo.StageInfo.HexUnitList.Find(a => a.HexField.X == FieldX.Value && a.HexField.Y == FieldY.Value && a.HexUnitType == HexUnitType.Basement);
            _reachPathes = _hexRoute.GetReachableArea(MoveType.Normal,hexUnit.HexField,1);
            var depaterIndex = 1000;
            foreach (var path in _reachPathes)
            {
                var unitData = new StageSymbolData
                {
                    InitX = path.X,
                    InitY = path.Y,
                    UnitType = HexUnitType.Reach
                };
                var depaterUnit = new HexUnitInfo(depaterIndex,unitData);
                CurrentGameInfo.StageInfo.AddHexUnitInfo(depaterUnit);
                depaterIndex++;
            }
        }

        public void MakeMoveBattlerHex()
        {
            var hexUnit = CurrentGameInfo.StageInfo.HexUnitList.Find(a => a.HexField.X == FieldX.Value && a.HexField.Y == FieldY.Value && a.IsUnit);
            SelectingHexUnitId.SetValue(hexUnit.Index.Value);
            _reachPathes = _hexRoute.GetReachableArea(MoveType.Normal,hexUnit.HexField,2);
            var moveBattlerIndex = 1000;
            foreach (var path in _reachPathes)
            {
                var unitData = new StageSymbolData
                {
                    InitX = path.X,
                    InitY = path.Y,
                    UnitType = HexUnitType.Reach
                };
                var moveBattlerUnit = new HexUnitInfo(moveBattlerIndex,unitData);
                CurrentGameInfo.StageInfo.AddHexUnitInfo(moveBattlerUnit);
                moveBattlerIndex++;
            }
        }

        public void SelectDeparture()
        {
            // 出撃する
            var depaterActorIndex = CurrentGameInfo.StageInfo.HexUnitList.FindAll(a => a.HexUnitType == HexUnitType.Battler).Count + 1;
            
            var unitData = new StageSymbolData
            {
                InitX = FieldX.Value,
                InitY = FieldY.Value,
                UnitType = HexUnitType.Battler,
            };
            var depaterActor = new HexUnitInfo(depaterActorIndex,unitData);
            depaterActor.SetActorInfos(new List<ActorInfo>(){StageMembers().Find(a => a.ActorId.Value == _departureActorId)});
            CurrentGameInfo.StageInfo.AddHexUnitInfo(depaterActor);

            // Reachを消去
            CurrentGameInfo.StageInfo.RemoveReachUnitInfo();
            _reachPathes.Clear();
            _selectActorId = -1;
        }
        
        public (List<Action>,HexUnitInfo) SelectMoveBattler()
        {
            var moveActions = new List<Action>();
            var pathes = new List<HexPath>();
            // 移動する
            var moveBattler = CurrentGameInfo.StageInfo.HexUnitList.Find(a => a.Index.Value == SelectingHexUnitId.Value && a.IsUnit);
            if (moveBattler != null)
            {
                var endHexUnit = new HexField
                {
                    X = FieldX.Value,
                    Y = FieldY.Value
                };
                // 移動ルート作成
                _hexRoute.FindRoute(MoveType.Normal,moveBattler.HexField,endHexUnit);
                pathes = _hexRoute.Pathlist;
                pathes.Reverse();
                foreach (var path in pathes)
                {
                    void action()
                    {
                        moveBattler.HexField.X = path.X;
                        moveBattler.HexField.Y = path.Y;
                    }
                    moveActions.Add(action);
                }
            }
            // Reachを消去
            CurrentGameInfo.StageInfo.RemoveReachUnitInfo();
            _reachPathes.Clear();
            return (moveActions,moveBattler);
        }

        public List<ListData> BattlerCommand()
        {
            var list = new List<SystemData.CommandData>();
            var move = new SystemData.CommandData
            {
                Id = 1,
                Name = "移動",
                Key = "MoveBattler"
            };
            list.Add(move);
            Func<SystemData.CommandData,bool> enable = (a) => 
            {
                return true;
            };
            return MakeListData(list,enable);
        }

        public List<ListData> BasementCommand()
        {
            var list = new List<SystemData.CommandData>();
            var departure = new SystemData.CommandData
            {
                Id = 1,
                Name = "出撃",
                Key = "Departure"
            };
            list.Add(departure);
            Func<SystemData.CommandData,bool> enable = (a) => 
            {
                return true;
            };
            return MakeListData(list,enable);
        }        
        
        public List<ListData> EndMoveBattlerCommand()
        {
            var list = new List<SystemData.CommandData>();
            var battle = new SystemData.CommandData
            {
                Id = 1,
                Name = "戦闘",
                Key = "Battle"
            };
            list.Add(battle);
            var wait = new SystemData.CommandData
            {
                Id = 2,
                Name = "待機",
                Key = "Wait"
            };
            list.Add(wait);
            Func<SystemData.CommandData,bool> enable = (a) => 
            {
                return true;
            };
            return MakeListData(list,enable);
        }
    }

    public class TacticsSceneInfo
    {
        // バトル直前に戻る
        public bool ReturnBeforeBattle;
        public bool ReturnNextBattle;
        public int SeekIndex = 0;
    }

    public class TacticsActorInfo
    {
        public ActorInfo ActorInfo;
        public List<ActorInfo> ActorInfos;
        public TacticsCommandType TacticsCommandType;
        public string DisableText;
    }

    public class TacticsCommandData
    {
        public string Title;
    }
}