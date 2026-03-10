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
            UIComponent.SetText(maxParam, max);
            UIComponent.SetText(currentParam, current);
        }

        private void UpdateCaptionText(StatusParamType statusParamType)
        {
            UIComponent.SetText(captionText, DataSystem.GetText(2100 + (int)statusParamType));
        }
    }
}
