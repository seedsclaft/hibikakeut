using System.Collections.Generic;

namespace Ryneus
{
    [System.Serializable]
    public class PartyInfo 
    {
        public PartyInfo()
        {
        }

        // 所持アクターリスト
        private List<ActorInfo> _actorInfos = new();
        public List<ActorInfo> ActorInfos => _actorInfos;
        public List<ActorInfo> GetActorInfos()
        {
            // ステージメンバー制限
            if (_stageId > 0)
            {
                var stageMaster = StageMaster;
                if (stageMaster != null && stageMaster.PartyMemberIds.Count > 0 && stageMaster.PartyMemberIds[0] != 0)
                {
                    return _actorInfos.FindAll(a => stageMaster.PartyMemberIds.Contains(a.ActorId));
                }
            }
            return _actorInfos;
        }

        // 現在のステージ場所
        private StageData StageMaster => DataSystem.FindStage(_stageId);
        private int _stageId = -1;
        public int StageId => _stageId;
        public void SetStageId(int stageId)
        {
            _stageId = stageId;
        }

        private int _seek = -1;
        public int Seek => _seek;
        public void SetSeek(int seek)
        {
            _seek = seek;
        }

        private int _seekIndex = 0;
        public int SeekIndex => _seekIndex;
        public void SetSeekIndex(int seekIndex)
        {
            _seekIndex = seekIndex;
        }

        // 所持金
        private int _currency = 0;
        public int Currency => _currency;
        public void AddCurrency(int currency)
        {
            _currency += currency;
            if (_currency < 0)
            {
                _currency = 0;
            }
        }

        // 所持アイテム情報
        private List<GetItemInfo> _getItemInfos = new ();
        public List<GetItemInfo> GetItemInfos => _getItemInfos;
        public void AddGetItemInfo(GetItemInfo getItemInfo)
        {
            _getItemInfos.Add(getItemInfo);
            CheckAddActor();
            CheckLearningSkillId();
        }

        private List<int> _learningSkillIds = new();
        public List<int> LearningSkillIds => _learningSkillIds;
        private void CheckLearningSkillId()
        {
            var addSkillInfos = _getItemInfos.FindAll(a => a.GetFlag && a.GetItemType == GetItemType.Skill);
            foreach (var addSkillInfo in addSkillInfos)
            {
                if (!_learningSkillIds.Contains(addSkillInfo.Param1))
                {
                    // 新規魔法入手
                    _learningSkillIds.Add(addSkillInfo.Param1);
                }
            }
        }

        private void CheckAddActor()
        {
            var addActorInfos = _getItemInfos.FindAll(a => a.GetFlag && a.GetItemType == GetItemType.AddActor);
            foreach (var addActorInfo in addActorInfos)
            {
                if (_actorInfos.Find(a => a.ActorId == addActorInfo.Param1) == null)
                {
                    // 新規加入
                    var actorData = DataSystem.FindActor(addActorInfo.Param1);
                    var actorInfo = new ActorInfo(actorData);
                    actorInfo.SetBattleIndex(_actorInfos.Count+1);
                    actorInfo.SetLevel(actorData.InitLv);
                    actorInfo.ChangeHp(actorInfo.MaxHp);
                    _actorInfos.Add(actorInfo);
                    // 整列
                    _actorInfos.Sort((a,b) => a.BattleIndex - b.BattleIndex > 0 ? 1 : -1);
                }
            }
        }
    }
}