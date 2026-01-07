using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class GetItemInfoComponent : MonoBehaviour
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private ItemInfoComponent itemInfoComponent;
        [SerializeField] private TextMeshProUGUI titleName;

        public void UpdateInfo(GetItemInfo getItemInfo)
        {
            UpdateData(getItemInfo.Master);
            if (titleName != null)
            {
                var title = getItemInfo.GetTitleData();
                titleName.gameObject.SetActive(title != "");
                titleName.SetText(title);
            }
        }

        public void UpdateData(GetItemData getItemData)
        {
            if (getItemData.Type == GetItemType.Skill && skillInfoComponent != null)
            {
                skillInfoComponent.UpdateData(getItemData.Param1);
            }
            else
            if (getItemData.Type == GetItemType.Item && itemInfoComponent != null)
            {
                itemInfoComponent.UpdateDate(DataSystem.Items.Find(a => a.Id == getItemData.Param1));
            }
            else
            {
            }
        }
    }
}
