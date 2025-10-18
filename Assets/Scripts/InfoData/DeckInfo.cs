using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    [System.Serializable]
    public class DeckInfo
    {
        public DeckInfo()
        {
            InitUnitInfos();
        }

        public ParameterInt Index = new();
        // 現在位置
        public ParameterInt StageNo = new();
        public ParameterInt TurnCount = new();
        public ParameterFloat GetDungeonBgmTimeStamp()
        {
            if (DungeonBgmTimeStamp == null)
            {
                DungeonBgmTimeStamp = new();
            }
            return DungeonBgmTimeStamp;
        }
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
        public string EncountRateText()
        {
            if (EncountRate.Value != 1)
            {

                return EncountRateTurn.Value + "ターン Encount率 x" + EncountRate.Value;
            }
            return "";
        }

        // 編成情報
        private Dictionary<int, int> _actorIdDict = new();
        public Dictionary<int, int> ActorIdDict => _actorIdDict;
        public void SetActorIdDict(Dictionary<int, int> actorIdDict) => _actorIdDict = actorIdDict;
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

        private void InitUnitInfos()
        {
            for (int i = 1;i <= 6;i++)
            {
                _actorIdDict[i] = -1;
            }
            //_actorIdDict[1] = 1;
        }

    }

    [System.Serializable]
    public class DungeonResumeInfo
    {
        public ParameterInt StageNo = new();
        public ParameterInt DungeonId = new();
        public ParameterInt PositionX = new();
        public ParameterInt PositionY = new();
        public ParameterInt Direction = new(); // 0=North, 1=East , 2=South , 3=West
    }
}
