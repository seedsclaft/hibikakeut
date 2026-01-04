using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public partial class BaseModel
    {
#if UNITY_ANDROID
        public List<RankingActorData> RankingActorDates()
        {
            var list = new List<RankingActorData>();
            foreach (var actorInfo in StageMembers())
            {
                var skillIds = new List<int>();
                foreach (var skill in actorInfo.Skills)
                {
                    skillIds.Add(skill.Id);
                }
                var rankingActorData = new RankingActorData()
                {
                    ActorId = actorInfo.ActorId,
                    Level = actorInfo.Level,
                    Hp = actorInfo.CurrentParameter(StatusParamType.Hp),
                    Mp = actorInfo.CurrentParameter(StatusParamType.Mp),
                    Atk = actorInfo.CurrentParameter(StatusParamType.Atk),
                    Def = actorInfo.CurrentParameter(StatusParamType.Def),
                    Spd = actorInfo.CurrentParameter(StatusParamType.Spd),
                    SkillIds = skillIds,
                    DemigodParam = actorInfo.DemigodParam,
                    Lost = actorInfo.Lost
                };
                list.Add(rankingActorData);
            }
            return list;
        }
#endif

        public async void CurrentRankingData(Action<string> endEvent)
        {
            var userId = CurrentData.PlayerInfo.UserId.ToString();
            var rankingText = "";
#if UNITY_WEBGL || UNITY_ANDROID && !UNITY_EDITOR
            FirebaseController.Instance.CurrentRankingData(userId);
            await UniTask.WaitUntil(() => FirebaseController.IsBusy == false);
            var currentScore = FirebaseController.CurrentScore;
            var evaluate = TotalScore;

            // 更新あり
            if (evaluate > currentScore)
            {
                var playerScore = (int)(evaluate * 100);
                FirebaseController.Instance.WriteRankingData(
                    CurrentStage.Id,
                    userId,
                    playerScore,
                    CurrentData.PlayerInfo.PlayerName,
                    StageMembers()
                );
                await UniTask.WaitUntil(() => FirebaseController.IsBusy == false);

                FirebaseController.Instance.ReadRankingData();
                await UniTask.WaitUntil(() => FirebaseController.IsBusy == false);
                var results = FirebaseController.RankingInfos;
                var rank = 1;
                var include = false;
                foreach (var result in results)
                {
                    if (result.Score == playerScore)
                    {
                        include = true;
                    }
                    if (result.Score > playerScore)
                    {
                        rank++;
                    }
                }

                if (include == true)
                {
                    // 〇位
                    rankingText = DataSystem.GetReplaceText(23030, rank.ToString());
                } else
                {
                    // 圏外
                    rankingText = DataSystem.GetText(23031);
                }
            } else
            {          
                // 記録更新なし  
                rankingText = DataSystem.GetText(23032);
            }
#endif
            endEvent(rankingText);
        }
    }
}
