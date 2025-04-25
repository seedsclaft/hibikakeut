using UnityEngine;
using System.Collections.Generic;

namespace Ryneus
{
    public class TacticsChecker : SingletonMonoBehaviour<TacticsChecker>
    {
        [SerializeField] private TeamInfo homeTeamInfo = null;
        [SerializeField] private List<HexUnitInfo> homeUnitInfos = null;
        [SerializeField] private List<HexUnitInfo> homeDepatuerInfos = null;
        [SerializeField] private TeamInfo awayTeamInfo = null;
        [SerializeField] private List<HexUnitInfo> awayUnitInfos = null;
        [SerializeField] private List<HexUnitInfo> awayDepatuerInfos = null;
        [SerializeField] private List<HexUnitInfo> stageFieldInfos = null;
        
        public void SetModel(StageInfo stageInfo)
        {
            if (stageInfo.HomeTeamInfo != null)
            {
                homeTeamInfo = stageInfo.HomeTeamInfo;
                homeUnitInfos = homeTeamInfo.UnitInfos;
                homeDepatuerInfos = homeTeamInfo.DepatuerInfos;
            }
            if (stageInfo.AwayTeamInfo != null)
            {
                awayTeamInfo = stageInfo.AwayTeamInfo;
                awayUnitInfos = awayTeamInfo.UnitInfos;
                awayDepatuerInfos = awayTeamInfo.DepatuerInfos;
            }
            if (stageInfo.FieldHexList != null)
            {
                stageFieldInfos = stageInfo.FieldHexList.FindAll(a => a.HexUnitType != HexUnitType.None);
            }
        }
    }
}
