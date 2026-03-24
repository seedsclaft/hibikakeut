using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

namespace Ryneus
{
    public class ActorInfoComponent : BaseInfoComponent
    {
        [SerializeField] private Image mainThumb;
        public Image MainThumb => mainThumb;
        [SerializeField] private Image awakenThumb;
        public Image AwakenThumb => awakenThumb;
        [SerializeField] private Image reliefThumb;
        [SerializeField] private Material grayscale;
        [SerializeField] private Image faceThumb;
        public Image FaceThumb => faceThumb;
        [SerializeField] private Image awakenFaceThumb;
        public Image AwakenFaceThumb => awakenFaceThumb;
        [SerializeField] private Image clipThumb;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI subNameText;
        [SerializeField] private TextMeshProUGUI profileText;
        [SerializeField] private TextMeshProUGUI evaluate;
        [SerializeField] private TextMeshProUGUI demigod;
        [SerializeField] private GameObject lvCation;
        [SerializeField] private TextMeshProUGUI lv;
        [SerializeField] private TextMeshProUGUI exp;
        [SerializeField] private Image expGauge;
        [SerializeField] private TextMeshProUGUI sp;
        [SerializeField] private StatusInfoComponent statusInfoComponent;
        [SerializeField] private StatusInfoComponent needStatusInfoComponent;

        [SerializeField] private TextMeshProUGUI element1;
        [SerializeField] private TextMeshProUGUI element2;
        [SerializeField] private TextMeshProUGUI element3;
        [SerializeField] private TextMeshProUGUI element4;
        [SerializeField] private TextMeshProUGUI element5;
        [SerializeField] private TextMeshProUGUI element6;

        [SerializeField] private TextMeshProUGUI element1Cost;
        [SerializeField] private TextMeshProUGUI element2Cost;
        [SerializeField] private TextMeshProUGUI element3Cost;
        [SerializeField] private TextMeshProUGUI element4Cost;
        [SerializeField] private TextMeshProUGUI element5Cost;
        [SerializeField] private TextMeshProUGUI element6Cost;

        [SerializeField] private TextMeshProUGUI recoveryCost;
        [SerializeField] private TextMeshProUGUI resourceGain;
        [SerializeField] private TextMeshProUGUI battlePosition;
        [SerializeField] private Image unitTypeImage;
        [SerializeField] private Image unitTypeImageBack;
        [SerializeField] private TextMeshProUGUI transferGetItemText;
        [SerializeField] private TextMeshProUGUI transferGetExpText;
        [SerializeField] private TextMeshProUGUI transferGetCurrencyText;
        [SerializeField] private Image kindIcon;
        [SerializeField] private TextMeshProUGUI kindText;

        public void UpdateInfo(ActorInfo actorInfo, List<ActorInfo> actorInfos)
        {
            if (actorInfo == null)
            {
                Clear();
                return;
            }
            var actorData = actorInfo.Master;

            UpdateData(actorData);
            if (mainThumb != null)
            {
                if (actorInfo.CurrentHp.Value == 0 && actorInfo.BattleIndex.Value >= 0)
                {
                    UpdateLostMainThumb();
                }
            }
            UIComponent.SetText(demigod, actorInfo.DemigodParam);
            UIComponent.SetActive(lvCation, true);
            UIComponent.SetText(lv, actorInfo.Level);
            UIComponent.SetText(exp, actorInfo.NextExp);
            if (expGauge != null)
            {
                expGauge.fillAmount = actorInfo.Exp.Value % 100 * 0.01f;
            }
            if (sp != null)
            {
            }
            if (statusInfoComponent != null && actorInfo.Master != null)
            {
                UIComponent.SetActive(statusInfoComponent.gameObject, true);
                statusInfoComponent.UpdateInfo(actorInfo.CurrentStatus);
                statusInfoComponent.UpdateHp(actorInfo.CurrentHp.Value, actorInfo.MaxHp);
                statusInfoComponent.UpdateMp(actorInfo.CurrentMp.Value, actorInfo.MaxMp);
                statusInfoComponent.UpdateCost(actorInfo.CurrentCost.Value, actorInfo.MaxCost);
            }
            if (needStatusInfoComponent != null)
            {
                needStatusInfoComponent.UpdateInfo(actorData.NeedStatus);
            }
            UpdateAttributeRank(element1, actorInfo, AttributeType.Fire, actorInfos);
            UpdateAttributeRank(element2, actorInfo, AttributeType.Thunder, actorInfos);
            UpdateAttributeRank(element3, actorInfo, AttributeType.Ice, actorInfos);
            UpdateAttributeRank(element4, actorInfo, AttributeType.Shine, actorInfos);
            UpdateAttributeRank(element5, actorInfo, AttributeType.Dark, actorInfos);
            UpdateAttributeRank(element6, actorInfo, AttributeType.Dark, actorInfos);

            UIComponent.SetText(element1Cost, actorInfo.LearningMagicCost(AttributeType.Fire, actorInfos).ToString());
            UIComponent.SetText(element2Cost, actorInfo.LearningMagicCost(AttributeType.Thunder, actorInfos).ToString());
            UIComponent.SetText(element3Cost, actorInfo.LearningMagicCost(AttributeType.Ice, actorInfos).ToString());
            UIComponent.SetText(element4Cost, actorInfo.LearningMagicCost(AttributeType.Shine, actorInfos).ToString());
            UIComponent.SetText(element5Cost, actorInfo.LearningMagicCost(AttributeType.Dark, actorInfos).ToString());
            UIComponent.SetText(element6Cost, actorInfo.LearningMagicCost(AttributeType.Void, actorInfos).ToString());

            UIComponent.SetText(recoveryCost, TacticsUtility.RemainRecoveryCost(actorInfo, true).ToString());
            UIComponent.SetText(resourceGain, TacticsUtility.ResourceGain(actorInfo).ToString());
            UIComponent.SetText(evaluate, DataSystem.GetReplaceDecimalText(actorInfo.Evaluate()));

            var textId = actorInfo.LineIndex == LineType.Front ? 2012 : 2013;
            UIComponent.SetText(battlePosition, DataSystem.GetText(textId));
            UIComponent.SetText(transferGetItemText, actorInfo.TransferGetItemText(PartyInfo.Period.Value));
            UIComponent.SetText(transferGetExpText, actorInfo.TransferGetExpText(PartyInfo.Chapter.Value, DataSystem.System.PeriodTurns - PartyInfo.Period.Value));

            UIComponent.SetText(transferGetCurrencyText, actorInfo.TransferGetCurrencyText(PartyInfo.Chapter.Value, DataSystem.System.PeriodTurns - PartyInfo.Period.Value));
        }

        private void UpdateAttributeRank(TextMeshProUGUI text, ActorInfo actorInfo, AttributeType attributeType, List<ActorInfo> actorInfos)
        {
            if (text == null)
            {
                return;
            }
            if (actorInfos != null)
            {
                UpdateAttributeParam(text, actorInfo.AttributeRanks(actorInfos)[(int)attributeType]);
            }
            else
            {
                UpdateAttributeParam(text, actorInfo.GetAttributeRank()[(int)attributeType]);
            }
        }

        public void UpdateData(ActorData actorData)
        {
            if (actorData == null)
            {
                Clear();
                return;
            }
            UpdateMainThumb(actorData.ImagePath, actorData.X, actorData.Y, actorData.Scale);
            UpdateAwakenThumb(actorData.ImagePath, actorData.AwakenX, actorData.AwakenY, actorData.AwakenScale);
            UpdateReliefThumb(actorData.ImagePath);
            UpdateClipThumb(actorData.ImagePath);
            UpdateMainFaceThumb(actorData.ImagePath);
            UpdateAwakenFaceThumb(actorData.ImagePath);
            UIComponent.SetText(nameText, actorData.Name);
            UIComponent.SetText(subNameText, actorData.SubName);
            UIComponent.SetText(profileText, actorData.Profile);
            if (kindIcon != null)
            {
                var kind = (int)actorData.AttributeType;
                if (kind > 0)
                {
                    UIComponent.SetActive(kindIcon.gameObject, true);
                    kindIcon.sprite = ResourceSystem.LoadElementIcon()[kind - 1];
                }
                UIComponent.SetActive(kindText, true);
                UIComponent.SetText(kindText, DataSystem.GetText(400 + kind - 1));
            }
            //UpdateUnitType(actorData.UnitType);
            //UpdateUnitTypeBack(actorData.UnitType);
        }

        private void UpdateMainThumb(string imagePath, int x, int y, float scale)
        {
            UIComponent.SetImage(mainThumb, ResourceSystem.ActorMainSpritePath(imagePath), () =>
            {
                var rect = mainThumb.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(x, y, 0);
                rect.localScale = new Vector3(scale, scale, 1);
                rect.sizeDelta = new Vector3(mainThumb.mainTexture.width, mainThumb.mainTexture.height, 1);
            });
        }

        private void UpdateAwakenThumb(string imagePath, int x, int y, float scale)
        {
            /*
            UIComponent.SetImage(awakenThumb, ResourceSystem.ActorAwakenSpritePath(imagePath), () =>
            {
                var rect = awakenThumb.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(x, y, 0);
                rect.localScale = new Vector3(scale, scale, 1);
                rect.sizeDelta = new Vector3(awakenThumb.mainTexture.width, awakenThumb.mainTexture.height, 1);
            });
            */
        }

        private void UpdateReliefThumb(string imagePath)
        {
            UIComponent.SetImage(reliefThumb, ResourceSystem.ActorReliefSpritePath(imagePath));
        }

        private void UpdateClipThumb(string imagePath)
        {
            UIComponent.SetImage(clipThumb, ResourceSystem.ActorClipSpritePath(imagePath));
        }

        private void UpdateMainFaceThumb(string imagePath)
        {
            UIComponent.SetImage(faceThumb, ResourceSystem.ActorMainFaceSpritePath(imagePath));
            UIComponent.SetActive(faceThumb, true);
        }

        private void UpdateAwakenFaceThumb(string imagePath)
        {
            UIComponent.SetImage(awakenFaceThumb, ResourceSystem.ActorAwakenSpritePath(imagePath));
            UIComponent.SetActive(awakenFaceThumb, true);
        }

        private void UpdateAttributeParam(TextMeshProUGUI textMeshProUGUI, AttributeRank param)
        {
            var textId = 2000 + (int)param;
            UIComponent.SetText(textMeshProUGUI, DataSystem.GetText(textId));
        }

        public void SetAwakeMode(bool IsAwaken)
        {
            if (faceThumb != null && awakenFaceThumb != null)
            {
                UIComponent.SetActive(faceThumb, !IsAwaken);
                UIComponent.SetActive(awakenFaceThumb, IsAwaken);
            }
        }

        private void UpdateLostMainThumb()
        {
            if (mainThumb != null && grayscale != null)
            {
                mainThumb.material = grayscale;
            }
        }

        public void LvupText(int plus)
        {
            int lvValue = int.Parse(lv.text) + plus;
            UIComponent.SetText(lv, lvValue);
        }

        public void Clear()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Dummy");
            UIComponent.SetImage(mainThumb, sprite);
            UIComponent.SetImage(awakenThumb, sprite);
            UIComponent.SetImage(faceThumb, sprite);
            UIComponent.SetImage(awakenFaceThumb, sprite);
            if (statusInfoComponent != null)
            {
                statusInfoComponent.Clear();
            }
            UIComponent.SetActive(lvCation, false);
            UIComponent.ClearText(lv);
            UIComponent.ClearText(nameText);
            UIComponent.ClearText(element1);
            UIComponent.ClearText(element2);
            UIComponent.ClearText(element3);
            UIComponent.ClearText(element4);
            UIComponent.ClearText(element5);
            UIComponent.ClearText(element6);
            UIComponent.ClearText(evaluate);
            UIComponent.SetActive(kindIcon, false);
            UIComponent.ClearText(kindText);
        }

        private void UpdateUnitType(UnitType unitType)
        {
            if (unitTypeImage == null)
            {
                return;
            }
            UIComponent.SetActive(unitTypeImage, true);
            var spriteAtlas = ResourceSystem.LoadUnitTypeIcons();
            UIComponent.SetImage(unitTypeImage, spriteAtlas.GetSprite(unitType.ToString()));
        }

        private void UpdateUnitTypeBack(UnitType unitType)
        {
            if (unitTypeImageBack == null)
            {
                return;
            }
            UIComponent.SetActive(unitTypeImageBack, true);
            var spriteAtlas = ResourceSystem.LoadUnitTypeBackIcons();
            unitTypeImageBack.sprite = spriteAtlas.GetSprite(unitType.ToString());
        }
    }
}
