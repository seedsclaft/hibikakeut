using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class StatusModel : BaseModel
    {
        private StatusViewInfo _sceneParam;
        public StatusViewInfo SceneParam => _sceneParam;
        private List<ActorInfo> _actorInfos = null;
        public List<ActorInfo> ActorInfos => _actorInfos;
        public StatusModel()
        {
            _sceneParam = (StatusViewInfo)GameSystem.SceneStackManager.LastStatusViewInfo;
            _actorInfos = _sceneParam.ActorInfos;
        }
        private SkillInfo _selectSkillInfo = null;
        public SkillInfo SelectSkillInfo => _selectSkillInfo;
        public void SetSelectSkillInfo(SkillInfo skillInfo) => _selectSkillInfo = skillInfo;

        public void ChangeEquipSkill(int changeSkillId)
        {
            CurrentActor.ChangeEquipSkill(changeSkillId,_selectSkillInfo.Id.Value);
        }

        public void RemoveEquipSkill(ActorInfo actorInfo,int removeSkillId)
        {
            actorInfo.ChangeEquipSkill(0,removeSkillId);
        }

        public void UpdateActorRemainCost()
        {
            var cost = 0;
            foreach (var slotSkill in EquipSkills())
            {
                cost += slotSkill.LearningCost.Value;
            }
            CurrentActor.ChangeCost(CurrentActor.MaxCost - cost);
        }

        public List<SkillInfo> EquipSkills()
        {
            return EquipSkills(CurrentActor);
        }

        public List<SkillInfo> ChangeAbleSkills()
        {
            // マイナスSP計算
            var cost = _selectSkillInfo != null ? CurrentActor.EquipSkillCost(_selectSkillInfo.Master.Id,PartyInfo.ActorInfos) : 0;
            var changeAbleSkills = ChangeAbleSkills(CurrentActor,cost);
            // はずすを挿入
            var removeSkill = new SkillInfo(1);
            removeSkill.SetEnable(true);
            changeAbleSkills.Insert(0,removeSkill);
            return changeAbleSkills;
        }

        public ActorInfo EquipmentSkill(SkillInfo skillInfo)
        {
            if (skillInfo.Master.SkillType != SkillType.Equipment)
            {
                return null;
            }
            var find = PartyInfo.ActorInfos.Find(a => a.EquipmentSkillIds.Contains(skillInfo.Id));
            return find;
        }

        public string HelpText()
        {
            return DataSystem.GetText(18010);
        }

        public ParameterInt CurrentIndex = new();
        public void SelectActor(int actorId)
        {
            var index = _actorInfos.FindIndex(a => a.ActorId.Value == actorId);
            CurrentIndex.SetValue(index);
        }

        public ActorInfo CurrentActor => _actorInfos[CurrentIndex.Value];

        public void ChangeActorIndex(int value)
        {
            CurrentIndex.GainValue(value,0,_actorInfos.Count-1,true);
        }

        public void SetActorLastSkillId(int selectSkillId)
        {
            CurrentActor.SetLastSelectSkillId(selectSkillId);
        }

        public List<ActorInfo> MakeSelectActorInfos()
        {
            return new List<ActorInfo>(){CurrentActor};
        }

        public List<GetItemInfo> MakeSelectGetItemInfos()
        {
            /*
            var getItemInfos = CurrentSelectRecord().SymbolInfo.GetItemInfos.FindAll(a => a.GetItemType == GetItemType.AddActor);
            var getItemInfo = getItemInfos.Find(a => a.Param1 == CurrentActor.ActorId);
            if (getItemInfo != null)
            {
                getItemInfo.SetResultParam(CurrentActor.ActorId);
                return new List<GetItemInfo>(){getItemInfo};
            }
            getItemInfos = CurrentSelectRecord().SymbolInfo.GetItemInfos.FindAll(a => a.GetItemType == GetItemType.SelectAddActor);
            if (getItemInfos.Count > 0)
            {
                getItemInfos[0].SetResultParam(CurrentActor.ActorId);
                return getItemInfos;
            }
            */
            return new List<GetItemInfo>(){};
        }



        public List<SkillTriggerInfo> SkillTrigger(int selectIndex = -1)
        {
            return CurrentActor.SkillTriggerInfos;
        }

        public List<SystemData.CommandData> StatusCommand()
        {
            return DataSystem.StatusCommand;
        }

        public int LevelUpCost()
        {
            return ActorLevelUpCost(CurrentActor);
        }

        public StrategySceneInfo DecideActor()
        {
            var getItemData = new GetItemData
            {
                Type = GetItemType.AddActor,
                Param1 = CurrentActor.ActorId.Value
            };
            var getItemInfo = new GetItemInfo(getItemData);
            AddGetItemInfo(getItemInfo);
            var strategySceneInfo = new StrategySceneInfo
            {
                ActorInfos = StageMembers().FindAll(a => a.ActorId.Value == CurrentActor.ActorId.Value),
                InBattle = false,
                ReturnScene = Scene.Dungeon
            };
            strategySceneInfo.GetItemInfos = new List<GetItemInfo>
            {
                getItemInfo
            };
            return strategySceneInfo;
        }
    }
}