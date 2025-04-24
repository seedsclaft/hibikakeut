using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class TeamInfoComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI actPointTotal;
        [SerializeField] private TextMeshProUGUI actPoint;
        public void UpdateInfo(TeamInfo teamInfo)
        {
            if (teamInfo == null)
            {
                return;
            }
            actPointTotal?.SetText(teamInfo.ActPoint.Value.ToString());
            actPoint?.SetText(teamInfo.CurrentActPoint.Value.ToString());
        }
    }
}
