using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class StatusInfoComponent : MonoBehaviour
    {
        [SerializeField] private StatusParameter hpParam;
        [SerializeField] private StatusParameter mpParam;
        [SerializeField] private StatusParameter atkParam;
        [SerializeField] private StatusParameter defParam;
        [SerializeField] private StatusParameter spdParam;
        [SerializeField] private StatusParameter costParam;

        [SerializeField] private StatusGaugeAnimation hpGaugeAnimation;
        [SerializeField] private StatusGaugeAnimation mpGaugeAnimation;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color upperColor;
        [SerializeField] private Color downColor;
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private int _rectWidth = 80;
        public void UpdateInfo(StatusInfo statusInfo, StatusInfo baseStatus = null)
        {
            if (statusInfo == null)
            {
                return;
            }
            if (hpParam != null)
            {
                hpParam.UpdateInfo(statusInfo.HpParam);
            }
            if (mpParam != null)
            {
                mpParam.UpdateInfo(statusInfo.MpParam);
            }
            if (atkParam != null)
            {
                atkParam.UpdateInfo(statusInfo.AtkParam);
                if (baseStatus != null)
                {
                    //ChangeTextColor(atk, statusInfo.Atk, baseStatus.Atk);
                }
            }
            if (defParam != null)
            {
                defParam.UpdateInfo(statusInfo.DefParam);
                if (baseStatus != null)
                {
                    //ChangeTextColor(atk, statusInfo.Atk, baseStatus.Atk);
                }
            }
            if (spdParam != null)
            {
                spdParam.UpdateInfo(statusInfo.SpdParam);
                if (baseStatus != null)
                {
                    //ChangeTextColor(atk, statusInfo.Atk, baseStatus.Atk);
                }
            }
            if (costParam != null)
            {
                costParam.UpdateInfo(statusInfo.CostParam);
                if (baseStatus != null)
                {
                    //ChangeTextColor(atk, statusInfo.Atk, baseStatus.Atk);
                }
            }
        }

        private void ChangeTextColor(TextMeshProUGUI text, int currentStatus, int baseStatus)
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

        public void UpdateHp(int currentHp, int maxStatusHp)
        {
            if (currentHp < 0)
            {
                currentHp = 0;
            }
            if (currentHp > maxStatusHp)
            {
                currentHp = maxStatusHp;
            }
            if (hpParam != null)
            {
                hpParam.UpdateParamter(currentHp, maxStatusHp);
            }
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

        public void UpdateMp(int currentMp, int maxStatusMp)
        {
            if (currentMp < 0)
            {
                currentMp = 0;
            }
            if (currentMp > maxStatusMp)
            {
                currentMp = maxStatusMp;
            }
            if (mpParam != null)
            {
                mpParam.UpdateParamter(currentMp, maxStatusMp);
            }
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

        public void UpdateCost(int currentCost, int maxStatusCost)
        {
            if (currentCost < 0)
            {
                currentCost = 0;
            }
            if (currentCost > maxStatusCost)
            {
                currentCost = maxStatusCost;
            }
            if (costParam != null)
            {
                costParam.UpdateParamter(currentCost, maxStatusCost);
            }
        }

        public void UpdateHpAnimation(int fromHp, int currentHp, int maxStatusHp)
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

        public void UpdateMpAnimation(int fromMp, int currentMp, int maxStatusMp)
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
            if (atkParam != null)
            {
                atkParam.UpdateParamter(value, value);
            }
        }

        public void UpdateDef(int value)
        {
            if (defParam != null)
            {
                defParam.UpdateParamter(value, value);
            }
        }

        public void UpdateSpd(int value)
        {
            if (spdParam != null)
            {
                spdParam.UpdateParamter(value, value);
            }
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
    }
}