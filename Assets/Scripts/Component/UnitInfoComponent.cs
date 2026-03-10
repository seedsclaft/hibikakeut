using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace Ryneus
{
    public class UnitInfoComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI noText;
        [SerializeField] private BattlerInfoComponent frontBattler;
        public BattlerInfoComponent FrontBattler => frontBattler;
        [SerializeField] private BattlerInfoComponent backBattler;
        public BattlerInfoComponent BackBattler => backBattler;


        public void UpdateInfo(UnitInfo unitInfo)
        {
            UIComponent.SetText(noText, "部隊"+unitInfo.Index.Value.ToString());
            if (frontBattler != null)
            {
                frontBattler.gameObject.SetActive(unitInfo.FrontBattlerInfo() != null);
                frontBattler.UpdateInfo(unitInfo.FrontBattlerInfo());
            }
            if (backBattler != null)
            {
                backBattler.gameObject.SetActive(unitInfo.BackBattlerInfo() != null);
                backBattler.UpdateInfo(unitInfo.BackBattlerInfo());
            }
        }

        public void HealAnimation(UnitInfo unitInfo,List<int> hpHeals)
        {
            UpdateInfo(unitInfo);
            if (hpHeals.Count > 0)
            {
                frontBattler.SetDamageRoot(gameObject);
                frontBattler.StartHeal(DamageType.HpHeal,hpHeals[0],false);
            }
            if (hpHeals.Count > 1)
            {
                backBattler.SetDamageRoot(gameObject);
                backBattler.StartHeal(DamageType.HpHeal,hpHeals[1],false);
            }
        }
    }
}
