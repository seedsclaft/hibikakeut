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

            UIComponent.SetActive(AwakenObj, data != null && data.Master.SkillType == SkillType.Awaken);
            UIComponent.SetActive(MessiahObj, data != null && data.Master.SkillType == SkillType.Unique);
            UIComponent.SetActive(BgObj, data != null && data.Master.SkillType != SkillType.Unique && data.Master.SkillType != SkillType.Awaken);
            UIComponent.SetActive(DisableSkill, data != null && !data.Enable);
        }

        public void Clear()
        {
            skillInfoComponent.Clear();
        }
    }
}