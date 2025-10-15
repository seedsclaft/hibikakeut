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
                if (skillInfo.FeatureDates.Count > 0)
                {
                    skillInfoComponent.UpdateInfo(skillInfo);
                } else
                {
                    skillInfoComponent.Clear();
                }
            } else
            {
                skillInfoComponent.Clear();
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            skillInfoComponent.gameObject.SetActive(true);
            UpdateSkillHelp();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            skillInfoComponent.gameObject.SetActive(false);
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
