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
        [SerializeField] private List<Sprite> symbolSprites;
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
            _hexUnitInfo = symbolInfo;
            if (_hexUnitInfo == null)
            {
                Clear();
                return;
            }
            if (selectArea != null)
            {
                selectArea.SetActive(_hexUnitInfo.IsSelectArea);
            }
            if (attackableArea != null)
            {
                attackableArea.SetActive(_hexUnitInfo.IsAttackableArea);
            }
            UpdateSymbolImage();
            if (_hexUnitInfo.UnitInfo != null && _hexUnitInfo.UnitInfo.BattlerInfos != null)
            {
                var battlerInfo = _hexUnitInfo.UnitInfo.BattlerInfos[0];
                battlerInfoComponent.UpdateInfo(battlerInfo);
                battlerInfoComponent.RefreshStatus();
            }
            UpdateFieldText();
            UpdateEvaluate();
            if (getItemList != null)
            {
                getItemList.SetData(MakeGetItemListData(),false);
            }
        }

        private void UpdateSymbolImage()
        {
            if (_hexUnitInfo.HexUnitType == HexUnitType.None) 
            {
                gameObject.SetActive(false);
                return;
            }
            //if (_hexUnitInfo.HexUnitType == HexUnitType.Random) return;
            gameObject.SetActive(true);
            if (_hexUnitInfo.IsBattlerUnit())
            {
                symbolImage?.gameObject.SetActive(false);
                enemyImage?.gameObject.SetActive(true);
                if (enemyImage != null)
                {
                    if (_hexUnitInfo.UnitInfo != null && _hexUnitInfo.UnitInfo.BattlerInfos != null)
                    {
                        battlerInfoComponent?.UpdateInfo(_hexUnitInfo.UnitInfo.BattlerInfos[0]);
                    }
                }
            } else
            {
                if (_hexUnitInfo.HexUnitType == HexUnitType.Reach) return;
                if (_hexUnitInfo.HexUnitType == HexUnitType.ReachAttack) return;
                if (symbolImage != null && symbolSprites != null)
                {
                    symbolImage?.gameObject.SetActive(true);
                    enemyImage?.gameObject.SetActive(false);
                    symbolImage.sprite = symbolSprites[(int)_hexUnitInfo.HexUnitType / 10];
                }
                if (_hexUnitInfo.HexUnitType == HexUnitType.Basement)
                {
                    if (homeTeamColor != null)
                    {
                        homeTeamColor.SetActive(_hexUnitInfo.TeamId.Value == (int)TeamIdType.Home);
                    }
                    if (awayTeamColor != null)
                    {
                        awayTeamColor.SetActive(_hexUnitInfo.TeamId.Value == (int)TeamIdType.Away);
                    }
                }
            }
        }

        private void UpdateFieldText()
        {
            fieldText?.SetText(_hexUnitInfo.FieldText());
        }

        private void UpdateEvaluate()
        {
            if (evaluateRoot != null)
            {
                evaluateRoot.SetActive(_hexUnitInfo.IsBattlerUnit());
            }
            if (evaluate != null)
            {
                var value = _hexUnitInfo.BattleEvaluate();
                evaluate.text = DataSystem.System.GetTextData(51).Text + ":" + value.ToString();
            }
        }

        private List<ListData> MakeGetItemListData()
        {
            var list = new List<ListData>();
            foreach (var getItemInfo in _hexUnitInfo.GetItemInfos)
            {
                if (getItemInfo.GetItemType == GetItemType.None)
                {
                    continue;
                }
                if (getItemInfo.GetItemType == GetItemType.SelectRelic && getItemInfo.Param1 < 0)
                {
                    continue;
                }
                if (getItemInfo.GetItemType == GetItemType.SelectSkill)
                {
                    continue;
                }
                var data = new ListData(getItemInfo);
                //data.SetEnable(symbolInfo.Cleared != true || getItemInfo.GetItemType != GetItemType.Numinous);
                if (getItemInfo.GetItemType == GetItemType.Skill)
                {
                    /*
                    // 入手済みなら
                    if (partyInfo.CurrentAlchemyIdList(currentStageInfo.Id,currentStageInfo.Seek,currentStageInfo.WorldType).Contains(getItemInfo.Param1))
                    {
                        data.SetEnable(false);
                    }
                    if (partyInfo.CurrentAlcanaIdList(currentStageInfo.Id,currentStageInfo.Seek,currentStageInfo.WorldType).Contains(getItemInfo.Param1))
                    {
                        data.SetEnable(false);
                    }
                    */
                    
                } else
                if (getItemInfo.GetItemType == GetItemType.AddActor)
                {
                    /*
                    // 入手済みなら
                    if (partyInfo.CurrentActorIdList(currentStageInfo.Id,currentStageInfo.Seek,currentStageInfo.WorldType).Contains(getItemInfo.ResultParam))
                    {
                        data.SetEnable(false);
                    }
                    */
                }
                list.Add(data);
            }
            return list;
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
