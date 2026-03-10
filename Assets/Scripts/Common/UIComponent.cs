using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class UIComponent
    {
        public static void SetText(TextMeshProUGUI textMeshProUGUI, string text)
        {
            if (textMeshProUGUI == null)
            {
                return;
            }
            textMeshProUGUI.SetText(text);
        }

        public static void SetText(TextMeshProUGUI textMeshProUGUI, int intValue)
        {
            SetText(textMeshProUGUI, intValue.ToString());
        }

        public static void SetText(TextMeshProUGUI textMeshProUGUI, ParameterString parameter)
        {
            SetText(textMeshProUGUI, parameter.Value);
        }

        public static void SetText(TextMeshProUGUI textMeshProUGUI, ParameterInt parameter)
        {
            SetText(textMeshProUGUI, parameter.Value.ToString());
        }

        public static void ClearText(TextMeshProUGUI textMeshProUGUI)
        {
            if (textMeshProUGUI == null)
            {
                return;
            }
            textMeshProUGUI.SetText("");
        }
    }
}
