using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class StatusInfoComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI maxHp;
        [SerializeField] private TextMeshProUGUI hp;
        [SerializeField] private TextMeshProUGUI maxMp;
        [SerializeField] private TextMeshProUGUI mp;
        [SerializeField] private TextMeshProUGUI atk;
        [SerializeField] private TextMeshProUGUI def;
        [SerializeField] private TextMeshProUGUI spd;

        [SerializeField] private StatusGaugeAnimation hpGaugeAnimation;
        [SerializeField] private StatusGaugeAnimation mpGaugeAnimation;
        [SerializeField] private TextMeshProUGUI hpCaption;
        [SerializeField] private TextMeshProUGUI mpCaption;
        [SerializeField] private TextMeshProUGUI atkCaption;
        [SerializeField] private TextMeshProUGUI defCaption;
        [SerializeField] private TextMeshProUGUI spdCaption;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color upperColor;
        [SerializeField] private Color downColor;
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private int _rectWidth = 80;
        public void UpdateInfo(StatusInfo statusInfo,StatusInfo baseStatus = null)
        {
            if (statusInfo == null)
            {
                return;
            }
            maxHp?.SetText(statusInfo.Hp.ToString());
            maxMp?.SetText(statusInfo.Mp.ToString());
            if (atk != null)
            {
                atk?.SetText(statusInfo.Atk.ToString());
                if (baseStatus != null)
                {
                    ChangeTextColor(atk,statusInfo.Atk,baseStatus.Atk);
                }
            }
            if (def != null)
            {
                def?.SetText(statusInfo.Def.ToString());
                if (baseStatus != null)
                {
                    ChangeTextColor(def,statusInfo.Def,baseStatus.Def);
                }
            }
            if (spd != null)
            {
                spd?.SetText(statusInfo.Spd.ToString());
                if (baseStatus != null)
                {
                    ChangeTextColor(spd,statusInfo.Spd,baseStatus.Spd);
                }
            }
            if (hpCaption != null)
            {
                UpdateCaption(StatusParamType.Hp,hpCaption);
            }
            if (mpCaption != null)
            {
                UpdateCaption(StatusParamType.Mp,mpCaption);
            }
            if (atkCaption != null)
            {
                UpdateCaption(StatusParamType.Atk,atkCaption);
            }
            if (defCaption != null)
            {
                UpdateCaption(StatusParamType.Def,defCaption);
            }
            if (spdCaption != null)
            {
                UpdateCaption(StatusParamType.Spd,spdCaption);
            }
        }

        private void ChangeTextColor(TextMeshProUGUI text,int currentStatus,int baseStatus)
        {
            if (currentStatus > baseStatus)
            {
                text.color = upperColor;        
            } else
            if (currentStatus < baseStatus)
            {
                text.color = downColor;
            } else
            {
                text.color = normalColor;
            }
        }

        public void UpdateHp(int currentHp,int maxStatusHp)
        {
            if (currentHp < 0)
            {
                currentHp = 0;
            }
            if (currentHp > maxStatusHp)
            {
                currentHp = maxStatusHp;
            }
            hp?.SetText(currentHp.ToString());
            maxHp?.SetText(maxStatusHp.ToString());
            if (hpGaugeAnimation != null)
            {
                var rate = 0f;
                if (currentHp > 0 && maxStatusHp > 0)
                {
                    rate = currentHp / (float)maxStatusHp;
                }
                hpGaugeAnimation.UpdateGauge(rate);
            }
        }

        public void UpdateMp(int currentMp,int maxStatusMp)
        {
            if (currentMp < 0)
            {
                currentMp = 0;
            }
            if (currentMp > maxStatusMp)
            {
                currentMp = maxStatusMp;
            }
            mp?.SetText(currentMp.ToString());
            maxMp?.SetText(maxStatusMp.ToString());
            if (mpGaugeAnimation != null)
            {
                var rate = 0f;
                if (currentMp > 0 && maxStatusMp > 0)
                {
                    rate = currentMp / (float)maxStatusMp;
                }
                mpGaugeAnimation.SetGaugeAnimation(rate);
                mpGaugeAnimation.UpdateGauge(rate);
            }
        }

        public void UpdateHpAnimation(int fromHp,int currentHp,int maxStatusHp)
        {
            if (hpGaugeAnimation != null)
            {
                var fromRate = 0f;
                if (fromHp > 0 && maxStatusHp > 0)
                {
                    fromRate = fromHp / (float)maxStatusHp;
                }
                hpGaugeAnimation.SetGaugeAnimation(fromRate);
                var rate = 0f;
                if (currentHp > 0)
                {
                    rate = currentHp / (float)maxStatusHp;
                }
                hpGaugeAnimation.UpdateGaugeAnimation(rate);
            }
        }

        public void UpdateMpAnimation(int fromMp,int currentMp,int maxStatusMp)
        {
            if (mpGaugeAnimation != null)
            {
                var fromRate = 0f;
                if (fromMp > 0 && maxStatusMp > 0)
                {
                    fromRate = fromMp / (float)maxStatusMp;
                }
                mpGaugeAnimation.SetGaugeAnimation(fromRate);
                var rate = 0f;
                if (currentMp > 0)
                {
                    rate = currentMp / (float)maxStatusMp;
                }
                mpGaugeAnimation.UpdateGaugeAnimation(rate);
            }
        }

        public void UpdateAtk(int value)
        {
            atk?.SetText(value.ToString());
        }

        public void UpdateDef(int value)
        {
            def?.SetText(value.ToString());
        }

        public void UpdateSpd(int value)
        {
            spd?.SetText(value.ToString());
        }

        public void ShowStatus()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1.0f;
            }
        }

        public void HideStatus()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        private void UpdateCaption(StatusParamType statusParamType,TextMeshProUGUI caption)
        {
            var textId = 2100 + (int)statusParamType;
            caption?.SetText(DataSystem.GetText(textId));
        }
    }
}