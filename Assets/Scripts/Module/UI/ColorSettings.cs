using UnityEngine;

namespace Ryneus
{
    public class ColorSettings : MonoBehaviour
    {
        public Color NormalTextColor = Color.white;
        public Color StatusTextColor = Color.white;
        public Color CursorColor = Color.white;
        public Color PowerUpColor = Color.white;
        public Color PowerDownColor = Color.white;
        public Color SkillTriggerColor = Color.white;

        public Color GetColor(TextColorType textColorType)
        {
            switch (textColorType)
            {
                case TextColorType.Normal:
                    return NormalTextColor;
                case TextColorType.Status:
                    return StatusTextColor;
                case TextColorType.Cursor:
                    return CursorColor;
                case TextColorType.PowerUp:
                    return PowerUpColor;
                case TextColorType.PowerDown:
                    return PowerDownColor;
                case TextColorType.SkillTrigger:
                    return SkillTriggerColor;
            }
            return NormalTextColor;
        }

        public string GetColorTag(TextColorType textColorType)
        {
            return "<color=#" + ColorUtility.ToHtmlStringRGB(GetColor(textColorType)) + ">";
        }
    }

    public enum TextColorType
    {
        None,
        Normal,
        Status,
        Cursor,
        PowerUp,
        PowerDown,
        SkillTrigger,
    }
}
