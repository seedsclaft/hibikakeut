using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class GetItemInfoComponent : MonoBehaviour
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private ItemInfoComponent itemInfoComponent;
        [SerializeField] private EquipmentInfoComponent equipmentInfoComponent;
        [SerializeField] private TextMeshProUGUI titleName;
        [SerializeField] private GameObject itemIconRoot;

        public void UpdateInfo(GetItemInfo getItemInfo)
        {
            UpdateData(getItemInfo.Master);
            if (titleName != null)
            {
                var title = getItemInfo.GetTitleData();
                UIComponent.SetActive(titleName.gameObject, title != "");
                UIComponent.SetText(titleName, title);
            }
        }

        public void UpdateData(GetItemData getItemData)
        {
            if (getItemData.Type == GetItemType.Skill && skillInfoComponent != null)
            {
                UIComponent.SetActive(itemIconRoot, true);
                skillInfoComponent.UpdateData(getItemData.Param1);
            }
            else
            if (getItemData.Type == GetItemType.Item && itemInfoComponent != null)
            {
                UIComponent.SetActive(itemIconRoot, true);
                itemInfoComponent.UpdateDate(DataSystem.FindItem(getItemData.Param1));
            }
            else
            if (getItemData.Type == GetItemType.Equipment && equipmentInfoComponent != null)
            {
                UIComponent.SetActive(itemIconRoot, true);
                equipmentInfoComponent.UpdateData(DataSystem.FindEquipment(getItemData.Param1));
            }
            else
            {
                UIComponent.SetActive(itemIconRoot, false);
            }
        }
    }
}
