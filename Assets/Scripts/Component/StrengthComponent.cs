using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace Ryneus
{
    public class StrengthComponent : MonoBehaviour
    {
        private ActorInfo _actorInfo = null;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI currentStatus;
        [SerializeField] private TextMeshProUGUI afterStatus;
        [SerializeField] private TextMeshProUGUI usePoint;

        public void UpdateInfo(ActorInfo actorInfo, StatusParamType statusParamType)
        {
            _actorInfo = actorInfo;

            var textData = DataSystem.GetText(2100 + (int)statusParamType);
            UIComponent.SetText(nameText, textData);

            var before = _actorInfo.LevelUpStatus(_actorInfo.Level-1).GetParameter(statusParamType);
            UIComponent.SetText(currentStatus, before.ToString());

            if (afterStatus != null)
            {
                var plus = _actorInfo.LevelUpStatus(_actorInfo.Level).GetParameter(statusParamType);
                UIComponent.SetActive(afterStatus, plus > before);
                UIComponent.SetText(afterStatus, plus);
            }
            int UseCost = _actorInfo.LevelGrowthRate(statusParamType,actorInfo.Level);
            UIComponent.SetText(usePoint, UseCost);
        }
    }
}