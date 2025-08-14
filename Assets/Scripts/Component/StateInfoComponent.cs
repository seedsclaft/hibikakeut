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
                description.text = effectText;
                var skill = DataSystem.FindSkill(stateInfo.SkillId.Value);
                if (skill != null)
                {
                    description.text = description.text + "(" + skill.Name + ")";
                }
            }
            if (turns != null)
            {
                turns.text = "";
                var removalTiming = stateInfo.RemovalTiming;
                if (removalTiming == RemovalTiming.UpdateTurn)
                {
                    if (stateInfo.Turns.Value > 900)
                    {
                        turns.text = DataSystem.GetText(403);
                    } else
                    {
                        turns.text = DataSystem.GetReplaceText(401,stateInfo.Turns.ToString());
                    }
                } else
                if (removalTiming == RemovalTiming.UpdateCount)
                {
                    turns.text = DataSystem.GetReplaceText(404,stateInfo.Turns.ToString());
                } else
                if (removalTiming == RemovalTiming.UpdateAp)
                {
                    if (stateInfo.Turns.Value > 900)
                    {
                        turns.text = DataSystem.GetText(403);
                    } else
                    {
                        turns.text = DataSystem.GetReplaceText(405,stateInfo.Turns.ToString());
                    }
                }
            }
        }

        public void UpdateData(StateType stateType)
        {
            if (stateType == 0)
            {
                return;
            }

            var stateData = DataSystem.States.Find(a => a.StateType == stateType);
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
            if (nameText != null)
            {
                nameText.SetText(stateData.Name);
            }
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