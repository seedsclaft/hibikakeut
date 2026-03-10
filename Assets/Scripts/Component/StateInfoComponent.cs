using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class StateInfoComponent : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image iconBack;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI turns;

        public void UpdateInfo(StateInfo stateInfo)
        {
            if (stateInfo == null)
            {
                return;
            }
            UpdateData(stateInfo.StateType);
            if (iconBack != null)
            {
                if (stateInfo.Master.IconBack > 0)
                {
                    iconBack.sprite = ResourceSystem.LoadStateIconBase(stateInfo.Master.IconBack);
                } else
                if (stateInfo.Master.Buff)
                {
                    iconBack.sprite = ResourceSystem.LoadStateIconBase(0);
                } else
                if (stateInfo.Master.DeBuff)
                {
                    iconBack.sprite = ResourceSystem.LoadStateIconBase(17);
                }
            }
            if (description != null)
            {
                string effectText = stateInfo.Master.Help.Replace("\\d",stateInfo.Effect.ToString());
                UIComponent.SetText(description, effectText);
                var skill = DataSystem.FindSkill(stateInfo.SkillId.Value);
                if (skill != null)
                {
                    UIComponent.SetText(description, description.text + "(" + skill.Name + ")");
                }
            }
            if (turns != null)
            {
                UIComponent.ClearText(turns);
                var removalTiming = stateInfo.RemovalTiming;
                switch (removalTiming)
                {
                    case RemovalTiming.UpdateTurn:
                        if (stateInfo.Turns.Value > 900)
                        {
                            // 永続
                            UIComponent.SetText(turns, DataSystem.GetText(2410));
                        } else
                        {
                            // 〇ターン
                            UIComponent.SetText(turns, DataSystem.GetReplaceText(2420, stateInfo.Turns.ToString()));
                        }
                        break;
                    case RemovalTiming.UpdateCount:
                        // 〇回
                        UIComponent.SetText(turns, DataSystem.GetReplaceText(2430, stateInfo.Turns.ToString()));
                        break;
                    case RemovalTiming.UpdateAp:
                        if (stateInfo.Turns.Value > 900)
                        {
                            UIComponent.SetText(turns, DataSystem.GetText(2410));
                        }
                        else
                        {
                            // 〇フレーム
                            UIComponent.SetText(turns, DataSystem.GetReplaceText(2440, stateInfo.Turns.ToString()));
                        }
                        break;
                }
            }
        }

        public void UpdateData(StateType stateType)
        {
            if (stateType == 0)
            {
                return;
            }

            var stateData = DataSystem.FindState((int)stateType);
            if (stateData == null)
            {
                return;
            }

            if (icon != null)
            {
                if (stateData.IconIndex == 0)
                {
                    UpdateStateIcon(stateData.IconPath);
                } else
                {
                    icon.sprite = ResourceSystem.LoadSBuffIcon(stateData.IconIndex);
                }
            }
            UIComponent.SetText(nameText, stateData.Name);
        }

        private void UpdateStateIcon(string iconPath)
        {
            var spriteAtlas = ResourceSystem.LoadIcons();
            if (icon != null)
            {
                icon.sprite = spriteAtlas.GetSprite(iconPath);
            }
        }
    }
}