using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class MagicList : BaseList
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private ScrollRect scrollHelp;
        public new void Initialize()
        {
            base.Initialize();
            SetSelectedHandler(() => UpdateSkillHelp());
        }

        public void UpdateSkillHelp()
        {
            if (skillInfoComponent == null)
            {
                return;
            }
            var listData = ListData;
            if (listData != null)
            {
                var skillInfo = (SkillInfo)listData.Data;
                if (skillInfo.FeatureDates.Count > 0 || skillInfo.Master.SkillType == SkillType.Kind)
                {
                    skillInfoComponent.UpdateInfo(skillInfo);
                }
                else
                {
                    skillInfoComponent.Clear();
                }
            }
            else
            {
                skillInfoComponent.Clear();
            }
        }

        public void Show()
        {
            UIComponent.SetActive(gameObject, true);
            UIComponent.SetActive(skillInfoComponent?.gameObject, true);
            UpdateSkillHelp();
        }

        public void Hide()
        {
            UIComponent.SetActive(gameObject, false);
            UIComponent.SetActive(skillInfoComponent?.gameObject, false);
        }

        public override void UpdateHelpWindow()
        {
            UpdateSkillHelp();
        }

        public void ScrollUpSkillHelp()
        {
            if (scrollHelp == null)
            {
                return;
            }
            var y = scrollHelp.verticalNormalizedPosition;
            y += 0.1f;
            scrollHelp.verticalNormalizedPosition = y;
        }

        public void ScrollDownSkillHelp()
        {
            if (scrollHelp == null)
            {
                return;
            }
            var y = scrollHelp.verticalNormalizedPosition;
            y -= 0.1f;
            scrollHelp.verticalNormalizedPosition = y;
        }
    }
}
