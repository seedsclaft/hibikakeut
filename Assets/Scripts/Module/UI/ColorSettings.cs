using UnityEngine;

namespace Ryneus
{
    public class ColorSettings : MonoBehaviour
    {
        public Color DummyColor = Color.white;
        public Color NormalTextColor = Color.white;
        public Color StatusTextColor = Color.white;
        public Color CursorColor = Color.white;
        public Color PowerUpColor = Color.white;
        public Color PowerDownColor = Color.white;
        public Color SkillTriggerColor = Color.white;
        public Color SkillTypeActiveColor = Color.white;
        public Color SkillTypePassiveColor = Color.white;
        public Color SkillTypeUniqueColor = Color.white;
        public Color SkillTypeAwakenColor = Color.white;
        public Color SkillTypeArtifactColor = Color.white;
        public Color SkillTypeEnhanceColor = Color.white;
        public Color SkillTypeEquipColor = Color.white;
        public Color SkillTypeKindColor = Color.white;
        public Color SkillTypeEquipmentColor = Color.white;
        public Color SkillTypeUseAlcanaColor = Color.white;

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
        
        public Color GetSkillBgColor(SkillType skillType)
        {
            switch (skillType)
            {
                case SkillType.Active:
                    return SkillTypeActiveColor;
                case SkillType.Passive:
                    return SkillTypePassiveColor;
                case SkillType.Unique:
                    return SkillTypeUniqueColor;
                case SkillType.Awaken:
                    return SkillTypeAwakenColor;
                case SkillType.Artifact:
                    return SkillTypeArtifactColor;
                case SkillType.Enhance:
                    return SkillTypeEnhanceColor;
                case SkillType.Equip:
                    return SkillTypeEquipColor;
                case SkillType.Kind:
                    return SkillTypeKindColor;
                case SkillType.Equipment:
                    return SkillTypeEquipmentColor;
                case SkillType.UseAlcana:
                    return SkillTypeUseAlcanaColor;
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
