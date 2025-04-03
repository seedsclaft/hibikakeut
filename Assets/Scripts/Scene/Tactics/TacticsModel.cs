using System;
using System.Collections.Generic;

namespace Ryneus
{
    public partial class TacticsModel : BaseModel
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
    }

    public class TacticsSceneInfo
    {
        // バトル直前に戻る
        public bool ReturnBeforeBattle;
        public bool ReturnNextBattle;
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