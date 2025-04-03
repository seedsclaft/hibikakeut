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
        [UnityEngine.SerializeField] private List<ActorInfo> _actorInfos = new();
        public List<ActorInfo> ActorInfos => _actorInfos;
        public List<ActorInfo> GetActorInfos()
        {
            return _actorInfos;
        }

        // 現在のステージ場所
        public StageData StageMaster => DataSystem.FindStage(StageId.Value);
        public ParameterInt StageId = new();
        public ParameterBool StartStage = new ();

        // 所持金
        public ParameterInt Currency = new();

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
                if (_actorInfos.Find(a => a.ActorId.Value == addActorInfo.Param1) == null)
                {
                    // 新規加入
                    var actorData = DataSystem.FindActor(addActorInfo.Param1);
                    var actorInfo = new ActorInfo(actorData);
                    actorInfo.BattleIndex.SetValue(_actorInfos.Count+1);
                    actorInfo.SetLevel(actorData.InitLv);
                    actorInfo.ChangeHp(actorInfo.MaxHp);
                    _actorInfos.Add(actorInfo);
                    // 整列
                    _actorInfos.Sort((a,b) => a.BattleIndex.Value - b.BattleIndex.Value > 0 ? 1 : -1);
                }
            }
        }
    }
}