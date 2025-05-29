using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class HexUnitComponent : MonoBehaviour
    {
        [SerializeField] private Image symbolImage;
        [SerializeField] private Image enemyImage;
        [SerializeField] private GameObject selectArea;
        [SerializeField] private GameObject attackableArea;
        [SerializeField] private GameObject evaluateRoot;
        [SerializeField] private TextMeshProUGUI evaluate;
        [SerializeField] private GameObject selected;
        [SerializeField] private GameObject homeTeamColor;
        [SerializeField] private GameObject awayTeamColor;
        [SerializeField] private TextMeshProUGUI fieldText;
        [SerializeField] private BaseList getItemList = null;
        [SerializeField] private BattlerInfoComponent battlerInfoComponent;
        public BaseList GetItemList => getItemList;
        private HexUnitInfo _hexUnitInfo  = null;
        public HexUnitInfo HexUnitInfo => _hexUnitInfo;


        public void Initialize()
        {
            getItemList?.Initialize();
        }

        public void UpdateInfo(HexUnitInfo symbolInfo)
        {
        }

        public void Clear()
        {
            symbolImage?.gameObject.SetActive(false);
            enemyImage?.gameObject.SetActive(false);
            if (selectArea != null)
            {
                selectArea?.SetActive(false);
            }
            if (attackableArea != null)
            {
                attackableArea?.SetActive(false);
            }
            if (homeTeamColor != null)
            {
                homeTeamColor?.SetActive(false);
            }
            if (awayTeamColor != null)
            {
                awayTeamColor?.SetActive(false);
            }
            gameObject.SetActive(true);
        }

        public void LostUnit()
        {
            if (battlerInfoComponent != null)
            {
                battlerInfoComponent.StartDeathAnimation();
            }
        }

        public void InitUnit()
        {
            if (battlerInfoComponent != null)
            {
                battlerInfoComponent.EndDeathAnimation();
            }
        }

        public void HealAnimation(int heHeal)
        {
            if (battlerInfoComponent != null)
            {
                battlerInfoComponent.SetDamageRoot(gameObject);
                battlerInfoComponent.StartHeal(DamageType.HpHeal,heHeal,false);
            }
        }
    }
}
