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
            if (ownNum != null)
            {
                ownNum.SetText(itemInfo.OwnNum.Value.ToString());
            }
            if (useNum != null)
            {
                useNum.SetText(itemInfo.UseNum.Value.ToString());
            }
        }

        private void UpdateDate(ItemData itemData)
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
            if (itemName != null)
            {
                itemName.SetText(itemData.Name);
            }
            if (description != null)
            {
                description.SetText(itemData.Help);
            }
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
            if (itemName != null)
            {
                itemName.SetText("");
            }
            if (description != null)
            {
                description.SetText("");
            }
        }
    }
}
