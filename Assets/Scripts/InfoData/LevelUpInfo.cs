using System.Collections;
using System.Collections.Generic;
using System;

namespace Ryneus
{
    [Serializable]
    public class LevelUpInfo
    {
        private bool _enable = true;
        public bool Enable => _enable;
        public void SetEnable(bool enable)
        {
            _enable = enable;
        }
        private int _actorId = -1;
        public int ActorId => _actorId;
        private int _level = -1;
        public int Level => _level;
        public void SetLevel(int level)
        {
            _level = level;
        }
        private int _skillId = -1;
        public int SkillId => _skillId;
        public void SetSkillId(int skillId)
        {
            _skillId = skillId;
        }
        private int _currency = 0;
        public int Currency => _currency;
        private int _stageId = -1;
        public int StageId => _stageId;

        public LevelUpInfo(int actorId,int currency,int stageId)
        {
            _actorId = actorId;
            _currency = currency;
            _stageId = stageId;
        }


        public bool IsSameLevelUpInfo(LevelUpInfo levelUpInfo)
        {
            return levelUpInfo.ActorId == _actorId && levelUpInfo.SkillId == _skillId && levelUpInfo.Level == _level && levelUpInfo.StageId == _stageId;
        }

        public bool IsLevelUpData()
        {
            return _enable && _skillId == -1;
        }

        public bool IsLearnSkillData()
        {
            return _enable && _skillId > -1;
        }

        public bool IsBattleResultData()
        {
            return IsLevelUpData() && _stageId > -1 && _currency == 0;
        }

        public bool IsTrainData()
        {
            return IsLevelUpData() && _currency > 0;
        }

    }
}
