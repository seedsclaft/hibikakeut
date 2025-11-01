using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class StatusParameter : MonoBehaviour
    {
        [SerializeField] private StatusParamType statusParamType;
        [SerializeField] private TextMeshProUGUI captionText;
        [SerializeField] private TextMeshProUGUI maxParam;
        [SerializeField] private TextMeshProUGUI currentParam;
        public void UpdateInfo(StatusParamInfo statusParamInfo)
        {
            UpdateCaptionText(statusParamInfo.paramType);
            UpdateParamter(statusParamInfo.StatusCurernt, (int)statusParamInfo.max.Value);
        }

        public void UpdateParamter(int current, int max)
        {
            if (maxParam != null)
            {
                maxParam.SetText(max.ToString());
            }
            if (currentParam != null)
            {
                currentParam.SetText(current.ToString());
            }
        }

        private void UpdateCaptionText(StatusParamType statusParamType)
        {
            if (captionText == null)
            {
                return;
            }
            captionText.SetText(DataSystem.GetText(2100 + (int)statusParamType));
        }
    }
}
