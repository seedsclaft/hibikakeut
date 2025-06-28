using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public partial class BaseModel
    {

        public List<SystemData.CommandData> ConfirmCommand()
        {
            return BaseConfirmCommand(3050,3051);
        }

        public List<SystemData.CommandData> NoChoiceConfirmCommand()
        {
            return new List<SystemData.CommandData>(){BaseConfirmCommand(3052,0)[0]};
        }

        public List<SkillInfo> SkillActionList(ActorInfo actorInfo)
        {
            return new List<SkillInfo>();
        }

        public List<SkillInfo> SkillActionListData(ActorInfo actorInfo)
        {
            return SkillActionList(actorInfo);
        }

        public List<SkillInfo> ActorLearningMagicList(ActorInfo actorInfo,int selectAttribute = -1, int selectedSkillId = -1)
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
            return skillInfos;
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
