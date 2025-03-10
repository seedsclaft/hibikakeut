using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public partial class BaseModel 
    {
        public List<ListData> MakeListData<T>(List<T> dataList)
        {
            var listData = ListData.MakeListData(dataList);
            return listData;
        }

        public List<ListData> MakeListData<T>(List<T> dataList,Func<T,bool> enable,int selectIndex = 0)
        {
            var listData = ListData.MakeListData(dataList,enable);
            return listData;
        }

        public List<ListData> ConfirmCommand()
        {
            return MakeListData(BaseConfirmCommand(3050,3051));
        }

        public List<ListData> NoChoiceConfirmCommand()
        {
            return MakeListData(new List<SystemData.CommandData>(){BaseConfirmCommand(3052,0)[0]});
        }

        public List<SkillInfo> SkillActionList(ActorInfo actorInfo)
        {            
            return new List<SkillInfo>();
            /*
            if (actorInfo == null)
            {
                return new List<SkillInfo>();
            }
            var alchemyIds = PartyInfo.CurrentAlchemyIdList(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType);
            return actorInfo.SkillActionList(alchemyIds);
            */
        }

        public List<ListData> SkillActionListData(ActorInfo actorInfo)
        {
            var data = SkillActionList(actorInfo);
            return MakeListData(data,(a) => { return true;},-1);
        }

        public List<ListData> ActorLearningMagicList(ActorInfo actorInfo,int selectAttribute = -1, int selectedSkillId = -1)
        {
            var skillInfos = new List<SkillInfo>();
            /*
            foreach (var alchemyId in PartyInfo.CurrentAlchemyIdList(CurrentStage.Id,CurrentStage.Seek,CurrentStage.WorldType))
            {
                var skillInfo = new SkillInfo(alchemyId);
                if (selectAttribute > 0)
                {
                    if ((int)skillInfo.Master.Attribute != selectAttribute)
                    {
                        continue;
                    }
                }
                var cost = TacticsUtility.LearningMagicCost(actorInfo,skillInfo.Attribute,StageMembers(),skillInfo.Master.Rank);
                skillInfo.SetEnable(Currency >= cost && !actorInfo.IsLearnedSkill(alchemyId));
                skillInfo.SetLearningCost(cost);
                skillInfos.Add(skillInfo);
            }
            */
            var selectIndex = skillInfos.FindIndex(a => a.Id.Value == selectedSkillId);
            var listData = MakeListData(skillInfos);
            return listData;
        }
        
        public List<AttributeType> AttributeTabList()
        {
            var list = new List<AttributeType>();
            foreach (var attribute in Enum.GetValues(typeof(AttributeType)))
            {
                var attributeType = (AttributeType)attribute;
                list.Add(attributeType);
            }
            return list;
        }
    }
}
