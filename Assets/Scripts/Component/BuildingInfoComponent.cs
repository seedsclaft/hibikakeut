using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class BuildingInfoComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI cost;

        public void UpdateData(BuildingsData buildingsData)
        {
            nameText?.SetText(buildingsData.Name);
            description?.SetText(buildingsData.Help);
            cost?.SetText(buildingsData.Cost.ToString() + DataSystem.GetText(1000));
            image.sprite = ResourceSystem.LoadBuildingSprite(buildingsData.ImagePath);
        }

        public void UpdateInfo(BuildingInfo buildingInfo)
        {
            UpdateData(buildingInfo.Master);
        }
    }
}
