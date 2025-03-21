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

        public bool IsCurrentSeekSymbolInfo(SymbolInfo symbolInfo)
        {
            return symbolInfo?.Master.Seek == PartyInfo.Seek.Value;
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

        public SymbolInfo SelectedSymbol()
        {
            int seekIndex = CurrentStage.SeekIndex.Value;
            return CurrentStage.SymbolInfos.Find(a => a.Master.Seek == PartyInfo.Seek.Value && a.Master.SeekIndex == seekIndex);
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

        /// <summary>
        /// 表示するステージデータ
        /// </summary>
        /// <returns></returns>
        public List<ListData> SymbolRecords()
        {
            var symbolInfos = new List<SymbolInfo>();
            var recordList = new Dictionary<int,List<SymbolResultInfo>>();
            
            var stageSeekList = new List<int>();
            /*
            var selectRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId > 0);
            selectRecords = selectRecords.FindAll(a => a.WorldType == CurrentStage.WorldType);
            // ブランチは始点と終点を作る
            if (CurrentStage.WorldType == WorldType.Brunch)
            {
                selectRecords = selectRecords.FindAll(a => a.IsBeforeStageSeek(returnSymbol.StageId,returnSymbol.Seek,WorldType.Brunch) && a.IsAfterStageSeek(brunchSymbol.StageId,brunchSymbol.Seek,WorldType.Brunch));
            }
            // 現在を挿入
            var currentSeek = CurrentStage.Seek;
            foreach (var selectRecord in selectRecords)
            {
                var stageKey = (selectRecord.StageId-1)*100 + selectRecord.Seek;
                if (!stageSeekList.Contains(stageKey))
                {
                    stageSeekList.Add(stageKey);
                }
            }    
            stageSeekList.Sort((a,b) => a - b > 0 ? 1 : -1);
            
            foreach (var stageSeek in stageSeekList)
            {
                var list = new List<SymbolResultInfo>();
                recordList[stageSeek] = new List<SymbolResultInfo>();
            }
            var lastSelectSeek =selectRecords.Select(a => a.Seek).Max();
            foreach (var selectRecord in selectRecords)
            {
                var stageKey = (selectRecord.StageId-1)*100 + selectRecord.Seek;
                if (recordList.ContainsKey(stageKey))
                {
                    recordList[stageKey].Add(selectRecord);
                }
            }
            var currentSymbol = new StageSymbolData
            {
                StageId = CurrentStage.Id,
                Seek = currentSeek,
                SeekIndex = 0,
                SymbolType = SymbolType.None
            };
            var currentInfo = new SymbolInfo(currentSymbol);
            var currentResult = new SymbolResultInfo(currentInfo);
            currentResult.SetWorldType(CurrentStage.WorldType);
            currentInfo.SetLastSelected(true);
            var currentList = new List<SymbolResultInfo>(){currentResult};
    
            var resultList = new List<List<SymbolResultInfo>>();
            var result = recordList.OrderBy(a => a.Key).ToList();
            foreach (var resultData in result)
            {
                resultList.Add(resultData.Value);
            }
            var currentIndex = resultList.FindIndex(a => a[0].IsSameStageSeek(CurrentStage.Id,currentSeek,CurrentStage.WorldType));
            if (currentIndex > -1)
            {
                resultList.Insert(currentIndex, currentList);
            } else
            {
                resultList.Add(currentList);
            }
            */
            var listData = new List<ListData>();
            /*
            foreach (var record in resultList)
            {
                var list = new ListData(record);
                list.SetSelected(false);
                list.SetEnable(false);
                if (record.Find(a => a.IsSameStageSeek(CurrentStage.Id,currentSeek,CurrentStage.WorldType)) != null)
                {
                    list.SetSelected(true);
                }
                listData.Add(list);
            }
            */
            return listData;
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