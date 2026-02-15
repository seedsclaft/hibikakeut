using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class LevelUpModel : BaseModel
    {
        private LevelUpViewInfo _sceneParam;
        public LevelUpViewInfo SceneParam => _sceneParam;
        public LevelUpModel()
        {
            _sceneParam = (LevelUpViewInfo)GameSystem.SceneStackManager.LastPopupInfo.template;

        }

        public string TitleText()
        {
            return _sceneParam.Title.Value;
        }


        public ActorInfo LevelUpActorInfo()
        {
            return _sceneParam.ActorInfo;
        }

        public List<StrategyStrengthInfo> LevelUpDates()
        {
            return _sceneParam.StrategyStrengthInfos;
        }

        public void ClearLevelUpDates()
        {
            _sceneParam.StrategyStrengthInfos.Clear();
        }

        public string LearnSkillText()
        {
            return _sceneParam.LearnSkill.Value;
        }

        public List<SkillInfo> LearnSkillInfos()
        {
            return _sceneParam.SkillInfos;
        }

        public void ClearSkillInfo()
        {
            _sceneParam.ClearSkillInfo();
        }
    }

    public class LevelUpViewInfo
    {
        public ParameterString Title = new();
        public ParameterString LearnSkill = new();
        public ParameterInt From = new();
        public ParameterInt To = new();
        private ActorInfo _actorInfo;
        public ActorInfo ActorInfo => _actorInfo;
        private List<SkillInfo> _skillInfos = new();
        public List<SkillInfo> SkillInfos => _skillInfos;
        public List<StrategyStrengthInfo> StrategyStrengthInfos = new();
        public LevelUpViewInfo()
        {
        }

        public void SetActorInfo(ActorInfo actorInfo)
        {
            _actorInfo = actorInfo;
        }

        public void SetSkillInfos(List<SkillInfo> skillInfos)
        {
            _skillInfos = skillInfos;
        }

        public void ClearSkillInfo()
        {
            _skillInfos.Clear();
        }
    }
}