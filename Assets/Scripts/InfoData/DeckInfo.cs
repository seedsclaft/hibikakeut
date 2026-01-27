using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class DeckInfo
    {
        public DeckInfo()
        {
            InitUnitInfos();
        }

        public ParameterInt Index = new();
        // 現在位置
        public ParameterInt StageNo = new();
        // 歩数カウント
        public ParameterInt TurnCount = new();
        // 帰還できるかのフラグ
        public ParameterBool Cursed = new();
        public ParameterFloat DungeonBgmTimeStamp = new();
        public ParameterInt DungeonId = new();
        public ParameterInt PositionX = new();
        public ParameterInt PositionY = new();
        public ParameterInt Direction = new(); // 0=North, 1=East , 2=South , 3=West
        private List<RoutePath> _routePaths = new();
        public List<RoutePath> RoutePaths => _routePaths;
        public void SetRoutePaths(List<RoutePath> routePaths)
        {
            _routePaths = routePaths;
        }
        public void SetPosition(int dungeonId, int x, int y, int direction)
        {
            DungeonId.SetValue(dungeonId);
            PositionX.SetValue(x);
            PositionY.SetValue(y);
            Direction.SetValue(direction);
        }
        public bool ExistPlayerPosition(int eventId)
        {
            var floor = DataSystem.FindDungeonFloor(DungeonId.Value);
            return eventId == PositionX.Value + (PositionY.Value * floor.floorSizeVertical);
        }

        // ランダムエンカウント値
        public ParameterInt Encount = new();
        public ParameterFloat EncountRate = new(1);
        public ParameterFloat EncountRateTurn = new(0);
        public ParameterInt EncountTimes = new();
        public void SetEncountTimes(int times)
        {
            EncountTimes ??= new();
            EncountTimes.SetValue(times);
        }
        public int RemainEncountTimes()
        {
            return EncountTimes.Value;
        }
        public string EncountRateText()
        {
            return EncountRate.Value != 1 ? EncountRateTurn.Value + "ターン Encount率 x" + EncountRate.Value : "";
        }

        // 編成情報
        private Dictionary<int, int> _actorIdDict = new();
        public Dictionary<int, int> ActorIdDict => _actorIdDict;

        // 回復可能回数
        public ParameterInt RecoveryCount = new();

        public void InitUnitInfos()
        {
            if (DataSystem.System == null)
            {
                return;
            }
            for (int i = 1; i <= DataSystem.System.PartyMemberNum; i++)
            {
                _actorIdDict[i] = -1;
            }
        }

        public void TransferActorInfo(int actorId)
        {
            var removeEditIndex = FindEditIndex(actorId);
            _actorIdDict[removeEditIndex] = -1;
        }

        public void SwapBattler(int fromEditIndex, int toActorId)
        {
            var toEditIndex = FindEditIndex(toActorId);
            var beforeEditIndex = _actorIdDict[fromEditIndex];
            _actorIdDict[fromEditIndex] = toActorId;
            if (toEditIndex != -1)
            {
                _actorIdDict[toEditIndex] = beforeEditIndex;
            }
        }

        public void SwapBattler(int fromEditIndex, int toActorId, int toEditIndex)
        {
            //var toEditIndex = FindEditIndex(toActorId);
            var beforeEditIndex = _actorIdDict[fromEditIndex];
            _actorIdDict[fromEditIndex] = toActorId;
            if (toEditIndex != -1)
            {
                _actorIdDict[toEditIndex] = beforeEditIndex;
            }
        }

        public int FindEditIndex(int actorId)
        {
            foreach (var actorIdDict in _actorIdDict)
            {
                if (actorIdDict.Value == actorId)
                {
                    return actorIdDict.Key;
                }
            }
            return -1;
        }

        public bool AdjustEditIndexes()
        {
            var backOnlyEdit = new List<int>();
            foreach (var actorIdDict in _actorIdDict)
            {
                if (actorIdDict.Key < 4)
                {
                    continue;
                }
                if (actorIdDict.Value != -1 && _actorIdDict[actorIdDict.Key - 3] == -1)
                {
                    backOnlyEdit.Add(actorIdDict.Key);
                }
            }
            foreach (var backOnly in backOnlyEdit)
            {
                SwapBattler(backOnly - 3, _actorIdDict[backOnly]);
            }
            return backOnlyEdit.Count > 0;
        }

        public void SetAutoDeck(List<ActorInfo> actorInfos)
        {
            for (int i = 1; i <= actorInfos.Count; i++)
            {
                _actorIdDict[i] = actorInfos[i - 1].ActorId.Value;
            }
        }
    }

    [Serializable]
    public class DungeonResumeInfo
    {
        public ParameterInt StageNo = new();
        public ParameterInt DungeonId = new();
        public ParameterInt PositionX = new();
        public ParameterInt PositionY = new();
        public ParameterInt Direction = new(); // 0=North, 1=East , 2=South , 3=West
    }
}
