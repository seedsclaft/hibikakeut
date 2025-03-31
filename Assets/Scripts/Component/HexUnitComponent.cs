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
        [SerializeField] private List<Sprite> symbolSprites;
        [SerializeField] private GameObject evaluateRoot;
        [SerializeField] private TextMeshProUGUI evaluate;
        [SerializeField] private GameObject selected;
        [SerializeField] private BaseList getItemList = null;
        public BaseList GetItemList => getItemList;
        private HexUnitInfo _hexUnitInfo  = null;
        public HexUnitInfo HexUnitInfo => _hexUnitInfo;

        private bool _animationInit = false;

        public void Initialize()
        {
            getItemList?.Initialize();
        }

        public void UpdateInfo(HexUnitInfo  symbolInfo)
        {
            _hexUnitInfo = symbolInfo;
            if (_hexUnitInfo == null)
            {
                return;
            }
            UpdateSymbolImage();
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
                symbolImage.gameObject.SetActive(false);
                enemyImage.gameObject.SetActive(false);
                return;
            }
            if (_hexUnitInfo.HexUnitType == HexUnitType.Group) return;
            //if (_hexUnitInfo.HexUnitType == HexUnitType.Random) return;
            if (_hexUnitInfo.IsBattleSymbol())
            {
                symbolImage.gameObject.SetActive(false);
                enemyImage.gameObject.SetActive(true);
                enemyImage.sprite = ResourceSystem.LoadEnemySprite(_hexUnitInfo.TroopInfo.BossEnemy.EnemyData.ImagePath);
            } else
            {
                if (symbolImage != null && symbolSprites != null)
                {
                    symbolImage.gameObject.SetActive(true);
                    enemyImage.gameObject.SetActive(false);
                    if (_hexUnitInfo.HexUnitType == HexUnitType.SelectActor)
                    {
                        symbolImage.sprite = symbolSprites[(int)HexUnitType.Actor];
                    } else
                    {
                        symbolImage.sprite = symbolSprites[(int)_hexUnitInfo.HexUnitType / 10];
                    }
                }
            }
        }

        private void UpdateEvaluate()
        {
            if (evaluateRoot != null)
            {
                evaluateRoot.SetActive(_hexUnitInfo.HexUnitType == HexUnitType.Battler || _hexUnitInfo.HexUnitType == HexUnitType.Boss);
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
            symbolImage.gameObject.SetActive(false);
            enemyImage.gameObject.SetActive(false);
        }
    }
}
