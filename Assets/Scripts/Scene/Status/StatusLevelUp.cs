using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class StatusLevelUp : MonoBehaviour
    {
        [SerializeField] private OnOffButton levelUpButton = null;
        [SerializeField] private OnOffButton learnMagicButton = null;
        [SerializeField] private Button learnMagicBackButton = null;
        [SerializeField] private TextMeshProUGUI numinousText = null;
        [SerializeField] private TextMeshProUGUI lvUpCostText = null;
        [SerializeField] private TextMeshProUGUI toLvText = null;
        [SerializeField] private TextMeshProUGUI beforeExp = null;
        [SerializeField] private TextMeshProUGUI afterExp = null;
        [SerializeField] private GameObject levelUpObj = null;
        [SerializeField] private InputInfoComponent levelUpButtonKey = null;
        public void Initialize(Action levelUpEvent)
        {
            if (levelUpButton != null)
            {
                levelUpButton.OnClickAddListener(() =>
                {
                    if (!levelUpButton.gameObject.activeSelf)
                    {
                        return;
                    }
                    levelUpEvent?.Invoke();
                });
            }
            if (learnMagicButton != null)
            {
                learnMagicButton.OnClickAddListener(CallLearnMagic);
            }
            if (learnMagicBackButton != null)
            {
                learnMagicBackButton.onClick.AddListener(CallHideLearnMagic);
            }
            if (levelUpButtonKey != null)
            {
                levelUpButtonKey.UpdateGuideIcon(6);
            }
            SetLearnMagicButtonActive(false);
        }

        public void CallLearnMagic()
        {
            if (!learnMagicButton.gameObject.activeSelf)
            {
                return;
            }
            //var eventData = new StatusViewEvent(CommandType.ShowLearnMagic);
            //_commandData(eventData);

        }

        public void CallHideLearnMagic()
        {
            if (!learnMagicBackButton.gameObject.activeSelf)
            {
                return;
            }
            //var eventData = new StatusViewEvent(CommandType.HideLearnMagic);
            //_commandData(eventData);

        }

        public void SetLearnMagicButtonActive(bool IsActive)
        {
            learnMagicBackButton?.gameObject.SetActive(IsActive);
        }

        public void SetLvUpInfo(int cost,int currency)
        {
            numinousText.SetText(currency + DataSystem.GetText(1000));
            //lvUpCostText.SetText(cost.ToString());
        }

        public void SetLvUpExpInfo(int before,int after)
        {
            beforeExp.SetText(before.ToString());
            afterExp.SetText(after.ToString());
        }
    }
}
