using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class StrategyResult : ListItem, IListViewItem
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private TextMeshProUGUI titleName;
        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }
            var data = ListItemData<GetItemResultViewInfo>();
            if (data == null)
            {
                return;
            }
            skillInfoComponent?.UpdateData(data.SkillId);
            if (titleName != null)
            {
                titleName.gameObject.SetActive(data.Title.Value != "");
                titleName.SetText(data.Title.Value);
                titleName.rectTransform.sizeDelta = new Vector2(titleName.preferredWidth,titleName.preferredHeight);
            }
        }
    }

    public class GetItemResultViewInfo
    {
        private int _skillId;
        public int SkillId => _skillId;
        public void SetSkillId(int skillId) => _skillId = skillId;
        public ParameterString Title = new();

        public string GetCurrencyText(int gainCurrency)
        {
            return gainCurrency.ToString() + DataSystem.GetText(1000);
        }

        public string GetSkillText(SkillData skillData)
        {
            return DataSystem.GetReplaceText(20100, skillData.Name);
        }
    }
}
