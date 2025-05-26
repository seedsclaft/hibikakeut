using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class ItemInfoComponent : BaseInfoComponent
    {
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
            if (itemName != null)
            {
                itemName.SetText(itemData.Name);
            }
            if (description != null)
            {
                description.SetText(itemData.Help);
            }
        }
    }
}
