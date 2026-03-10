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
                icon.gameObject.SetActive(true);
                UpdateItemIcon(itemData.IconIndex);
            }
            if (iconBack != null)
            {
                iconBack.gameObject.SetActive(true);
                UpdateItemIconBack(itemData.ItemType, itemData.Param2);
            }
            UIComponent.SetText(itemName, itemData.Name);
            UIComponent.SetText(description, itemData.Help);
        }

        private void UpdateItemIcon(int iconIndex)
        {
            icon.gameObject.SetActive(true);
            if (icon != null)
            {
                icon.sprite = ResourceSystem.LoadSBuffIcon(iconIndex);
            }
        }

        private void UpdateItemIconBack(ItemType itemType, int attributeType)
        {
            iconBack.gameObject.SetActive(true);
            if (iconBack != null)
            {
                iconBack.sprite = ResourceSystem.LoadItemIconBase(itemType, (AttributeType)attributeType);
            }
        }

        public void Clear()
        {
            if (icon != null)
            {
                icon.gameObject.SetActive(false);
            }
            if (iconBack != null)
            {
                iconBack.gameObject.SetActive(false);
            }
            UIComponent.ClearText(itemName);
            UIComponent.ClearText(description);
        }
    }
}
