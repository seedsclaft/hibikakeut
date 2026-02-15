using UnityEngine;

namespace Ryneus
{
    public class DungeonPlayerCamera : MonoBehaviour
    {
        [SerializeField] Camera playerCamera = null;

        private void Update()
        {
            UpdatePlayerCamera();
        }

        private void UpdatePlayerCamera()
        {
            if (playerCamera == null)
            {
                return;
            }
            float defaultAspect = 16f / 9f;

            //実機でのアスペクト比
            float realAspect = (float)Screen.width / Screen.height;

            //実機と開発画面の比率
            float ratio = defaultAspect / realAspect;
            if (ratio > 1)
            {
                ratio = 1 + ((ratio - 1) / 2);
            }
            playerCamera.fieldOfView = 78 * ratio;
        }
    }
}
