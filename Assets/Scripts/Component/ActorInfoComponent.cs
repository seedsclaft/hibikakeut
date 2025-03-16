﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class ActorInfoComponent : MonoBehaviour
    {
        [SerializeField] private Image mainThumb;
        public Image MainThumb => mainThumb;
        [SerializeField] private Image awakenThumb;
        public Image AwakenThumb => awakenThumb;
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

        public void UpdateInfo(ActorInfo actorInfo,List<ActorInfo> actorInfos)
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
            demigod?.SetText(actorInfo.DemigodParam.ToString());
            lv?.SetText(actorInfo.Level.ToString());
            exp?.SetText(actorInfo.NextExp.ToString());
            if (expGauge != null)
            {
                expGauge.fillAmount = actorInfo.Exp.Value % 100 * 0.01f;
            }
            if (sp != null){
            }
            if (statusInfoComponent != null && actorInfo.Master != null)
            {
                statusInfoComponent.gameObject.SetActive(true);
                statusInfoComponent.UpdateInfo(actorInfo.CurrentStatus);
                statusInfoComponent.UpdateHp(actorInfo.CurrentHp.Value,actorInfo.MaxHp);
                statusInfoComponent.UpdateMp(actorInfo.CurrentMp.Value,actorInfo.MaxMp);
            }
            if (needStatusInfoComponent != null)
            {
                needStatusInfoComponent.UpdateInfo(actorData.NeedStatus);
            }
            if (element1 != null)
            {
                UpdateAttributeRank(element1,actorInfo,AttributeType.Fire,actorInfos);
            }
            if (element2 != null)
            {
                UpdateAttributeRank(element2,actorInfo,AttributeType.Thunder,actorInfos);
            }
            if (element3 != null)
            {
                UpdateAttributeRank(element3,actorInfo,AttributeType.Ice,actorInfos);
            }
            if (element4 != null)
            {
                UpdateAttributeRank(element4,actorInfo,AttributeType.Shine,actorInfos);
            }
            if (element5 != null)
            {
                UpdateAttributeRank(element5,actorInfo,AttributeType.Dark,actorInfos);
            }
            if (element6 != null)
            {
                UpdateAttributeRank(element6,actorInfo,AttributeType.Dark,actorInfos);
            }
            element1Cost?.SetText(actorInfo.LearningMagicCost(AttributeType.Fire,actorInfos).ToString());
            element2Cost?.SetText(actorInfo.LearningMagicCost(AttributeType.Thunder,actorInfos).ToString());
            element3Cost?.SetText(actorInfo.LearningMagicCost(AttributeType.Ice,actorInfos).ToString());
            element4Cost?.SetText(actorInfo.LearningMagicCost(AttributeType.Shine,actorInfos).ToString());
            element5Cost?.SetText(actorInfo.LearningMagicCost(AttributeType.Dark,actorInfos).ToString());
            element6Cost?.SetText(actorInfo.LearningMagicCost(AttributeType.Void,actorInfos).ToString());
            
            recoveryCost?.SetText(TacticsUtility.RemainRecoveryCost(actorInfo,true).ToString());
            resourceGain?.SetText(TacticsUtility.ResourceGain(actorInfo).ToString());
            evaluate?.SetText(actorInfo.Evaluate().ToString());
            if (battlePosition != null)
            {
                var textId = actorInfo.LineIndex == LineType.Front ? 2012 : 2013;
                battlePosition.text = DataSystem.GetText(textId);
            }
        }

        private void UpdateAttributeRank(TextMeshProUGUI text,ActorInfo actorInfo,AttributeType attributeType,List<ActorInfo> actorInfos)
        {
            if (actorInfos != null)
            {
                UpdateAttributeParam(text,actorInfo.AttributeRanks(actorInfos)[(int)attributeType-1]);
            } else
            {
                UpdateAttributeParam(text,actorInfo.GetAttributeRank()[(int)attributeType-1]);
            }
        }

        public void UpdateData(ActorData actorData)
        {
            if (actorData == null)
            {
                Clear();
                return;
            }
            UpdateMainThumb(actorData.ImagePath,actorData.X,actorData.Y,actorData.Scale);
            UpdateAwakenThumb(actorData.ImagePath,actorData.AwakenX,actorData.AwakenY,actorData.AwakenScale);
            UpdateClipThumb(actorData.ImagePath);
            UpdateMainFaceThumb(actorData.ImagePath);
            UpdateAwakenFaceThumb(actorData.ImagePath);
            nameText?.SetText(actorData.Name);
            subNameText?.SetText(actorData.SubName);
            profileText?.SetText(actorData.Profile);
            //UpdateUnitType(actorData.UnitType);
            //UpdateUnitTypeBack(actorData.UnitType);
        }

        private void UpdateMainThumb(string imagePath,int x,int y,float scale)
        {
            if (mainThumb == null)
            {
                return;
            }
            var handle = ResourceSystem.LoadActorMainSprite(imagePath);
            var rect = mainThumb.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(x, y, 0);
            rect.localScale = new Vector3(scale, scale, 1);
            mainThumb.sprite = handle;
            rect.sizeDelta = new Vector3(mainThumb.mainTexture.width, mainThumb.mainTexture.height, 1);
        }

        private void UpdateAwakenThumb(string imagePath,int x,int y,float scale)
        {
            if (awakenThumb == null)
            {
                return;
            }
            var handle = ResourceSystem.LoadActorAwakenSprite(imagePath);
            var rect = awakenThumb.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(x, y, 0);
            rect.localScale = new Vector3(scale, scale, 1);
            awakenThumb.sprite = handle;
            rect.sizeDelta = new Vector3(mainThumb.mainTexture.width, mainThumb.mainTexture.height, 1);
        }

        private void UpdateClipThumb(string imagePath)
        {
            if (clipThumb == null) 
            {
                return;
            }
            clipThumb.sprite = ResourceSystem.LoadActorClipSprite(imagePath);
        }

        private void UpdateMainFaceThumb(string imagePath)
        {   
            if (faceThumb == null)
            {
                return;
            }
            faceThumb.sprite = ResourceSystem.LoadActorMainFaceSprite(imagePath);
            faceThumb.gameObject.SetActive(true);
        }

        private void UpdateAwakenFaceThumb(string imagePath)
        {
            if (awakenFaceThumb == null)
            {
                return;
            }
            awakenFaceThumb.sprite = ResourceSystem.LoadActorAwakenFaceSprite(imagePath);
            awakenFaceThumb.gameObject.SetActive(true);
        }

        private void UpdateAttributeParam(TextMeshProUGUI textMeshProUGUI,AttributeRank param)
        {
            var textId = 2000 + (int)param;
            textMeshProUGUI?.SetText(DataSystem.GetText(textId));
        }
        
        public void SetAwakeMode(bool IsAwaken)
        {
            if (faceThumb != null && awakenFaceThumb != null)
            {
                faceThumb.gameObject.SetActive(!IsAwaken);
                awakenFaceThumb.gameObject.SetActive(IsAwaken);
            }
        }

        private void UpdateLostMainThumb()
        {
            if (mainThumb != null && grayscale != null)
            {
                mainThumb.material = grayscale;
            }
        }

        public void Clear()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Dummy");
            if (mainThumb != null)
            {
                mainThumb.sprite = sprite;
            }
            if (awakenThumb != null)
            {
                awakenThumb.sprite = sprite;
            }
            if (faceThumb != null)
            {
                faceThumb.sprite = sprite;
            }
            if (awakenFaceThumb != null)
            {
                awakenFaceThumb.sprite = sprite;
            }
            if (statusInfoComponent != null)
            {
                statusInfoComponent.gameObject.SetActive(false);
            }
        }

        private void UpdateUnitType(UnitType unitType)
        {
            if (unitTypeImage == null)
            {
                return;
            }
            unitTypeImage.gameObject.SetActive(true);
            var spriteAtlas = ResourceSystem.LoadUnitTypeIcons();
            unitTypeImage.sprite = spriteAtlas.GetSprite(unitType.ToString());
        }

        private void UpdateUnitTypeBack(UnitType unitType)
        {
            if (unitTypeImageBack == null)
            {
                return;
            }
            unitTypeImageBack.gameObject.SetActive(true);
            var spriteAtlas = ResourceSystem.LoadUnitTypeBackIcons();
            unitTypeImageBack.sprite = spriteAtlas.GetSprite(unitType.ToString());
        }
    }
}
