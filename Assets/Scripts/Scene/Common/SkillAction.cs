using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class SkillAction : ListItem, IListViewItem
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private GameObject BgObj;
        [SerializeField] private GameObject AwakenObj;
        [SerializeField] private GameObject MessiahObj;
        [SerializeField] private GameObject DisableSkill;

        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }

            var data = ListItemData<SkillInfo>();
            skillInfoComponent.UpdateInfo(data);
            if (AwakenObj != null)
            {
                AwakenObj?.SetActive(data != null && data.Master.SkillType == SkillType.Awaken);
            }
            if (MessiahObj != null)
            {
                MessiahObj?.SetActive(data != null && data.Master.SkillType == SkillType.Unique);
            }
            if (BgObj != null)
            {
                BgObj?.SetActive(data != null && data.Master.SkillType != SkillType.Unique && data.Master.SkillType != SkillType.Awaken);
            }
            if (DisableSkill != null)
            {
                DisableSkill?.SetActive(data != null && !data.Enable);
            }
        }

        public void Clear()
        {
            skillInfoComponent.Clear();
        }
    }
}