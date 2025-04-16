using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class UnitInfoComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI noText;

        public void UpdateInfo(UnitInfo unitInfo)
        {
            noText?.SetText(unitInfo.Index.Value.ToString());
        }
    }
}
