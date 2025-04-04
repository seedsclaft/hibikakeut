using System;
using System.Collections.Generic;

namespace Ryneus
{
    public class BattlePartyModel : BaseModel
    {
        public BattlePartySceneInfo SceneParam;


        private ActorInfo _currentActor = null;
        public ActorInfo CurrentActor => _currentActor;
        public void SetCurrentActorInfo(ActorInfo actorInfo) => _currentActor = actorInfo; 
        
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
            var fromIndex = _swapFromActor.BattleIndex;
            var toIndex = actorInfo.BattleIndex;
            _swapFromActor.BattleIndex = toIndex;
            actorInfo.BattleIndex = fromIndex;
        }

        public BattlePartyModel()
        {
            SceneParam = (BattlePartySceneInfo)GameSystem.SceneStackManager.LastTemplate;
            _currentActor = SceneParam.ActorInfos[0];
        }

        public List<SystemData.CommandData> BattlePartyCommand()
        {
            var list = new List<SystemData.CommandData>();
            var enemyInfo = new SystemData.CommandData
            {
                Id = 1,
                Name = DataSystem.GetText(19800),
                Key = "EnemyInfo"
            };
            list.Add(enemyInfo);
            var edit = new SystemData.CommandData
            {
                Id = 0,
                Name = DataSystem.GetText(30000),
                Key = "Edit"
            };
            list.Add(edit);
            /*
            var replay = new SystemData.CommandData
            {
                Id = 2,
                Name = DataSystem.GetText(30010),
                Key = "Replay"
            };
            list.Add(replay);
            */
            var battle = new SystemData.CommandData
            {
                Id = 3,
                Name = DataSystem.GetText(30020),
                Key = "Battle"
            };
            list.Add(battle);
            return list;
        }

        public List<ActorInfo> BattlePartyMembers()
        {
            /*
            var list = new List<ActorInfo>();
            var battleMembers = BattleMembers();
            foreach (var battleMember in battleMembers)
            {
                list.Add(battleMember);
            }
            var stageMembers = StageMembers();
            foreach (var stageMember in stageMembers)
            {
                if (!list.Contains(stageMember))
                {
                    list.Add(stageMember);
                }
            }
            */
            return BattleMembers();
        }

        public List<ListData> SelectActorLearningMagicList(int selectAttribute,int selectedSkillId = -1)
        {
            return ActorLearningMagicList(CurrentActor,selectAttribute,selectedSkillId);
        }
        

        public void SetPartyBattlerIdList()
        {
            var idList = new List<int>();
            foreach (var battleMember in BattleMembers())
            {
                idList.Add(battleMember.ActorId.Value);
            }
            //PartyInfo.SetLastBattlerIdList(idList);
        }

        public List<BattlerInfo> EnemyInfos()
        {
            return SceneParam.EnemyInfos;
        }

        public void SetInBattle()
        {
            var actorInfo = _currentActor;
            var battleIndex = StageMembers().FindAll(a => a.BattleIndex.Value >= 0).Count + 1;
            if (actorInfo.BattleIndex.Value >= 0)
            {
                RemoveBattleActor(actorInfo);
                return;
            }
            if (battleIndex > 5) 
            {
                return;
            }
            actorInfo.BattleIndex.SetValue(battleIndex);
        }
        
        private void RemoveBattleActor(ActorInfo actorInfo)
        {
            actorInfo.BattleIndex.SetValue(-1);
            var battleMembers = BattleMembers();
            for (int i = 0;i < battleMembers.Count;i++)
            {
                battleMembers[i].BattleIndex.SetValue(i + 1);
            }
        }

        public bool IsEnableBattleReplay()
        {
            return false;
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
    }
    
    public class BattlePartySceneInfo
    {
        public BattlePartySceneInfo()
        {
        }
        
        public bool IsBoss = false;
        public List<ActorInfo> ActorInfos;
        public List<BattlerInfo> EnemyInfos;
    }
}