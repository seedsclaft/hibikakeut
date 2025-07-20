using UnityEngine;

namespace Ryneus
{
    public class BattlerGrid : MonoBehaviour
    {
        [SerializeField] private BattlerInfoComponent battlerInfoComponent;
        [SerializeField] private CanvasGroup canvasGroup;

        public void UpdateInfo(BattlerInfo battlerInfo)
        {
            battlerInfoComponent.UpdateInfo(battlerInfo);
        }

        public void RefreshStatus()
        {
            battlerInfoComponent.RefreshStatus();
        }
    }
}