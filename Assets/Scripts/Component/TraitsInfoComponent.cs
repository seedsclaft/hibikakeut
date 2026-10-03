using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class TraitsInfoComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI lvText;
        [SerializeField] private TextMeshProUGUI oneParamText;
        [SerializeField] private TextMeshProUGUI allParamText;
        public void UpdateInfo(TraitsInfo traitsInfo)
        {
            UpdateData(traitsInfo.Master);
            UIComponent.SetText(lvText, traitsInfo.Level.Value);
            UIComponent.SetText(allParamText, traitsInfo.Master.Param * traitsInfo.Level.Value);
        }

        public void UpdateData(HeroicData heroicData)
        {
            UIComponent.SetText(nameText, heroicData.Name);
            UIComponent.SetText(oneParamText, "(" + heroicData.Param + ")");
        }
    }
}
