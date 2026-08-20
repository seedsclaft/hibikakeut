using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class ItemInfoComponent : BaseInfoComponent
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image iconBack;
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI ownNum;
        [SerializeField] private TextMeshProUGUI useNum;

        public void UpdateInfo(ItemInfo itemInfo)
        {
            UpdateDate(itemInfo.Master);
            UIComponent.SetText(ownNum, itemInfo.OwnNum);
            UIComponent.SetText(useNum, itemInfo.UseNum);
        }

        public void UpdateDate(ItemData itemData)
        {
            if (icon != null)
            {
                UIComponent.SetActive(icon, true);
                UpdateItemIcon(itemData.IconIndex);
            }
            if (iconBack != null)
            {
                UIComponent.SetActive(iconBack, true);
                UpdateItemIconBack(itemData.ItemType, itemData.Param2);
            }
            UIComponent.SetText(itemName, itemData.GetName());
            UIComponent.SetText(description, itemData.GetHelp());
        }

        private void UpdateItemIcon(int iconIndex)
        {
            UIComponent.SetActive(icon, true);
            if (icon != null)
            {
                icon.sprite = ResourceSystem.LoadSBuffIcon(iconIndex);
            }
        }

        private void UpdateItemIconBack(ItemType itemType, int attributeType)
        {
            UIComponent.SetActive(iconBack, true);
            if (iconBack != null)
            {
                iconBack.sprite = ResourceSystem.LoadItemIconBase(itemType, (AttributeType)attributeType);
            }
        }

        public void Clear()
        {
            UIComponent.SetActive(icon, false);
            UIComponent.SetActive(iconBack, false);
            UIComponent.ClearText(itemName);
            UIComponent.ClearText(description);
        }
    }
}
