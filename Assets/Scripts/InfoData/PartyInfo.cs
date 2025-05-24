using System.Collections.Generic;

namespace Ryneus
{
    [System.Serializable]
    public class PartyInfo
    {
        public PartyInfo()
        {
            InitDeckInfos();
            EvaluationValue.SetValue(100);
            Chapter.SetValue(1);
            Period.SetValue(1);
        }

        // 所持アクターリスト
        [UnityEngine.SerializeField] private List<ActorInfo> _actorInfos = new();
        public List<ActorInfo> ActorInfos => _actorInfos;

        [UnityEngine.SerializeField] private List<BattlerInfo> _battlerInfos = new();
        public List<BattlerInfo> BattlerInfos => _battlerInfos;

        // 現在のステージ場所
        public StageData StageMaster => DataSystem.FindStage(StageId.Value);
        public ParameterInt StageId = new();

        // 所持金
        public ParameterInt Currency = new();

        // フェーズ
        public ParameterInt Chapter = new();

        // フェーズ終了までのピリオド
        public ParameterInt Period = new();

        // 評価値
        public ParameterInt EvaluationValue = new();

        // 出撃回数
        public ParameterInt DepartureCount = new();

        // 報告リストのRank
        public ParameterInt MissionRank = new(1);

        private List<AchievementInfo> _achievements = new();
        public List<AchievementInfo> AchievementInfos => _achievements;
        public void SetAchievementRank(List<AchievementData> achievementDatas)
        {
            _achievements.Clear();
            foreach (var achievementData in achievementDatas)
            {
                if (achievementData.Rank == MissionRank.Value)
                {
                    var achievementInfo = new AchievementInfo(achievementData);
                    _achievements.Add(achievementInfo);
                }
            }
        }

        // 所持アイテム情報
        private List<GetItemInfo> _getItemInfos = new();
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

                    var battlerInfo = new BattlerInfo(actorInfo,actorInfo.BattleIndex.Value);
                    _battlerInfos.Add(battlerInfo);
                }
            }
        }

        public ParameterInt DeckId = new(1);
        private Dictionary<int,DeckInfo> _deckInfos = new();
        private void InitDeckInfos()
        {
            for (int i = 1;i <= 5;i++)
            {
                _deckInfos[i] = new DeckInfo();
            }
        }

        public DeckInfo CurrentDeckInfo => _deckInfos[DeckId.Value];

        public void MakeInitDeckInfo()
        {
            var initUnitInfo = new UnitInfo();
            initUnitInfo.Index.SetValue(1);
            var actorInfo = ActorInfos[0];
            var battlerInfo = new BattlerInfo(actorInfo,1);
            var battlerInfo2 = new BattlerInfo();
            initUnitInfo.SetBattlers(new List<BattlerInfo>(){battlerInfo,battlerInfo2});
            CurrentDeckInfo.AddUnitInfos(initUnitInfo);
        }

        public List<BattlerInfo> CurrentDeckBattlerInfos()
        {
            var battlerInfos = new List<BattlerInfo>();
            foreach (var actorId in CurrentDeckInfo.ActorIdDict)
            {
                var find = _battlerInfos.Find(a => a.ActorInfo.ActorId.Value == actorId.Value);
                if (find != null)
                {
                    battlerInfos.Add(find);
                }
            }
            return battlerInfos;
        }

        public List<ActorInfo> CurrentDeckActorInfos()
        {
            var actorInfos = new List<ActorInfo>();
            foreach (var actorId in CurrentDeckInfo.ActorIdDict)
            {
                var find = _actorInfos.Find(a => a.ActorId.Value == actorId.Value);
                if (find != null)
                {
                    actorInfos.Add(find);
                }
            }
            return actorInfos;
        }
    }
}