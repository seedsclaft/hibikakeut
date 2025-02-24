using System.Collections.Generic;
using System.Diagnostics;

namespace Ryneus
{
    public partial class BaseModel
    {
        public void MakeStageInfo(int stageId,int clearCount = 0)
        {
            var stageInfo = new StageInfo(stageId);
            foreach (var getItemInfo in StageOpeningGetItemInfos(stageId,clearCount))
            {
                AddGetItemInfo(getItemInfo);          
            }
            stageInfo.SetSymbolInfos(GetStageSymbolInfos(stageId,clearCount));
            CurrentGameInfo.SetStageInfo(stageInfo);
            PartyInfo.SetStageId(stageId);
            PartyInfo.SetSeek(1);
            PartyInfo.SetSeekIndex(0);
        }
        
        public List<GetItemInfo> StageOpeningGetItemInfos(int stageId,int clearCount)
        {
            var getItemInfos = new List<GetItemInfo>();
            var stageSymbolDates = DataSystem.FindStage(stageId).StageSymbols;
            stageSymbolDates = stageSymbolDates.FindAll(a => a.Seek == -1 && a.ClearCount <= clearCount);
            foreach (var stageSymbolData in stageSymbolDates)
            {
                var symbolInfo = new SymbolInfo(stageSymbolData);
                getItemInfos.AddRange(symbolInfo.GetItemInfos);
            }
            return getItemInfos;
        }

        public List<SymbolInfo> GetStageSymbolInfos(int stageId,int clearCount)
        {
            return StageSymbolInfos(DataSystem.FindStage(stageId).StageSymbols,clearCount);
        }

        /// <summary>
        /// 表示するステージデータ
        /// </summary>
        /// <returns></returns>
        public List<ListData> StageSymbolInfos()
        {
            var symbolListDict = new Dictionary<int,List<SymbolInfo>>();
            var symbolInfos = CurrentStage.SymbolInfos;

            foreach (var symbolInfo in symbolInfos)
            {
                if (!symbolListDict.ContainsKey(symbolInfo.Master.Seek))
                {
                    symbolListDict[symbolInfo.Master.Seek] = new List<SymbolInfo>();
                }
                symbolListDict[symbolInfo.Master.Seek].Add(symbolInfo);
            }

            var listData = new List<ListData>();
            foreach (var symbolList in symbolListDict)
            {
                var list = new ListData(symbolList.Value);
                listData.Add(list);
            }
            return listData;
        }

        public SymbolInfo CurrentSymbolInfo()
        {
            var symbolInfos = CurrentStage?.SymbolInfos;
            if (PartyInfo != null && symbolInfos != null)
            {
                return symbolInfos.Find(a => a.Master.Seek == PartyInfo.Seek && a.Master.SeekIndex == PartyInfo.SeekIndex);
            }
            return null;
        }

        public void EndSymbolInfo(SymbolInfo symbolInfo)
        {
            foreach (var getItemInfo in symbolInfo.GetItemInfos)
            {   
                AddGetItemInfo(getItemInfo);   
            }
        }

        public void SeekNext()
        {
            foreach (var actorInfo in PartyInfo.ActorInfos)
            {
                actorInfo.ChangeHp(actorInfo.MaxHp);
            }
            PartyInfo.SetSeek(PartyInfo.Seek + 1);
            PartyInfo.SetSeekIndex(0);
            // ステージ終了していたら次のステージへ
            if (PartyInfo.Seek > CurrentStage.EndSeek)
            {
                var next = DataSystem.FindNextStage(CurrentStage.Id);
                if (next != null)
                {
                    MakeStageInfo(next.Id,CurrentData.PlayerInfo.ClearCount);
                }
            }
        }
    }
}
