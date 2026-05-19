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

        private Color _beforeColor = new(0, 0, 1);
 
        void Awake()
        {
            if (statusParamType == StatusParamType.Hp || statusParamType == StatusParamType.Mp)
            {
                _beforeColor = maxParam.color;
            } else
            {
                _beforeColor = currentParam.color;
            }
        }
 
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

        public void ChangeTextColor(Color color)
        {
            if (statusParamType == StatusParamType.Hp || statusParamType == StatusParamType.Mp)
            {
                maxParam.color = color;
            } else
            {
                currentParam.color = color; 
            }
        }

        public void ResetTextColor()
        {
            if (statusParamType == StatusParamType.Hp || statusParamType == StatusParamType.Mp)
            {
                maxParam.color = _beforeColor;
            } else
            {
                currentParam.color = _beforeColor; 
            }
        }
    }
}
