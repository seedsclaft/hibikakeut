using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class AlcanaInfoComponent : BaseInfoComponent
    {
        [SerializeField] private GameObject alcana;
        [SerializeField] private TextMeshProUGUI alcanaCount;
        [SerializeField] private _2dxFX_Shiny_Reflect shinyReflect;
        [SerializeField] private InputInfoComponent alcanaButtonKey = null;
        public void UpdateCurrentInfo()
        {
            var current = PartyInfo;
            if (current != null)
            {
                UpdateInfo(current.AritifactSkills());
            }
        }

        public void UpdateInfo(List<SkillInfo> skillInfos)
        {
            UIComponent.SetActive(alcana, skillInfos.Count > 0);
            UIComponent.SetText(alcanaCount, skillInfos.Count);
            if (alcanaButtonKey != null)
            {
                alcanaButtonKey.UpdateGuideIcon(InputKeyType.SideRight1);
            }
        }
    }
}