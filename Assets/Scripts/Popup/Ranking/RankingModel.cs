using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Ryneus
{
    public class RankingModel : BaseModel
    {
        private int _stageId = 0;
        public async void RankingInfos(int stageId,Action<List<ListData>> endEvent)
        {
            _stageId = stageId;
            /*
            if (TempInfo.TempRankingData.ContainsKey(stageId) == false)
            {
                FirebaseController.Instance.ReadRankingData();
                await UniTask.WaitUntil(() => FirebaseController.IsBusy == false);
                
                // 結果をそのまま参照渡しにしないこと
                var list = new List<RankingInfo>();
                foreach (var rankingInfo in FirebaseController.RankingInfos)
                {
                    var rankingData = new RankingInfo();
                    rankingData.CopyInfo(rankingInfo);
                    list.Add(rankingData);
                }
                TempInfo.SetRankingInfo(stageId,list);
            }
            if (endEvent != null) 
            {
                var rankingDataList = MakeListData(TempInfo.TempRankingData[stageId]);
                foreach (var rankingData in rankingDataList)
                {
                    var data = (RankingInfo)rankingData.Data;
                    rankingData.SetSelected(data.Name == CurrentData.PlayerInfo.PlayerName && data.Score == PartyInfo.TotalScore(WorldType.Main));
                }
                endEvent(rankingDataList);
            }
            */
        }
    }
}