using System;
using System.Collections.Generic;

namespace Ryneus
{
    // セーブデータに保存しないデータ類を管理
    public class TempInfo
    {
        // プレイ時間
        public ParameterInt PlayingTime = new();
        public ParameterInt LastStartTime = new();
        private static DateTime BaseDateTime()
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public long LocalEpochTime()
        {
            return DateTimeToEpochTime(DateTime.UtcNow);
        }

        public static long DateTimeToEpochTime(DateTime dateTime)
        {
            return (long)(dateTime - BaseDateTime()).TotalSeconds;
        }

        public long NowEpochTime()
        {
            long diffTime = LocalEpochTime() - LastStartTime.Value;
            return diffTime;
        }

        // カラーデータ
        public ColorSettings ColorSettings = null;

        private List<ActorInfo> _tempActorInfos = new();
        // バトル前のアクターデータを設定
        public List<ActorInfo> TempActorInfos => _tempActorInfos;
        public void CashBattleActors(List<ActorInfo> actorInfos)
        {
            ClearBattleActors();
            foreach (var actorInfo in actorInfos)
            {
                var tempInfo = new ActorInfo(actorInfo.Master);
                //tempInfo.CopyData(actorInfo);
                _tempActorInfos.Add(tempInfo);
            }
        }

        public void ClearBattleActors()
        {
            _tempActorInfos.Clear();
        }

        private Dictionary<int, List<RankingInfo>> _tempRankingData = new();
        public Dictionary<int, List<RankingInfo>> TempRankingData => _tempRankingData;
        public void SetRankingInfo(int stageId, List<RankingInfo> rankingInfos)
        {
            _tempRankingData[stageId] = rankingInfos;
        }

        public void ClearRankingInfo()
        {
            _tempRankingData.Clear();
        }

        private InputType _tempInputType = InputType.All;
        public InputType TempInputType => _tempInputType;

        public void SetInputType(InputType inputType)
        {
            _tempInputType = inputType;
        }

        // リプレイデータ
        private SaveBattleInfo _clearPartyReplayData;
        public SaveBattleInfo ClearPartyReplayData => _clearPartyReplayData;
        public void SetSaveBattleInfo(SaveBattleInfo clearPartyReplayData)
        {
            _clearPartyReplayData = clearPartyReplayData;
        }

        private bool _inReplay = false;
        public bool InReplay => _inReplay;
        public void SetInReplay(bool inReplay)
        {
            _inReplay = inReplay;
        }

        private Dictionary<int, int> _weightKey = new();
        public Dictionary<int, int> EnqmySkillWeights(List<SkillInfo> skillInfos)
        {
            var weightKey = _weightKey; //skillId, weight
            var first = _weightKey.Count == 0;
            foreach (var enableSkillInfo in skillInfos)
            {
                if (!first && weightKey.ContainsKey(enableSkillInfo.Id.Value))
                {
                    continue;
                }
                foreach (var enemyData in DataSystem.Dates[DataType.Enemies].ToList<EnemyData>())
                {
                    foreach (var learningSkill in enemyData.LearningSkills)
                    {
                        if (learningSkill.SkillId != enableSkillInfo.Id.Value)
                        {
                            continue;
                        }
                        if (!weightKey.ContainsKey(enableSkillInfo.Id.Value) || weightKey[enableSkillInfo.Id.Value] < learningSkill.Weight)
                        {
                            weightKey[enableSkillInfo.Id.Value] = learningSkill.Weight;
                        }
                    }
                }
                if (!weightKey.ContainsKey(enableSkillInfo.Id.Value))
                {
                    weightKey[enableSkillInfo.Id.Value] = 0;
                }
            }
            return weightKey;
        }
    }
}