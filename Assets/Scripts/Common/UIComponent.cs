using UnityEngine.UI;
using TMPro;
using UnityEngine;

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

        public static void SetImage(Image image, string path, System.Action endCall = null)
        {
            if (image == null)
            {
                return;
            }
            var sprite = ResourceSystem.GetAsset<Sprite>(path);
            if (sprite != null)
            {
                image.sprite = sprite;
            }
            else
            {
                ResourceSystem.LoadAssetData<Sprite>(path, (result) =>
                {
                    image.sprite = result;
                    endCall?.Invoke();
                });
            }
        }

        public static void SetSpeiteImage(SpriteRenderer spriteRenderer, string path, System.Action endCall = null)
        {
            if (spriteRenderer == null)
            {
                return;
            }
            var sprite = ResourceSystem.GetAsset<Sprite>(path);
            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }
            else
            {
                ResourceSystem.LoadAssetData<Sprite>(path, (result) =>
                {
                    spriteRenderer.sprite = result;
                    endCall?.Invoke();
                });
            }
        }

        public static void SetImage(Image image, Sprite sprite)
        {
            if (image == null)
            {
                return;
            }
            image.sprite = sprite;
        }
    }
}
