using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    using Status;
    public class StatusLevelUp : MonoBehaviour
    {
        private System.Action<ViewEvent> _commandData = null;
        [SerializeField] private OnOffButton levelUpButton = null;
        [SerializeField] private OnOffButton learnMagicButton = null;
        [SerializeField] private Button learnMagicBackButton = null;
        [SerializeField] private TextMeshProUGUI numinousText = null;
        [SerializeField] private TextMeshProUGUI lvUpCostText = null;
        [SerializeField] private TextMeshProUGUI toLvText = null;
        [SerializeField] private GameObject levelUpObj = null;
        // Start is called before the first frame update
        public void Initialize()
        {
            levelUpButton?.OnClickAddListener(() => 
            {
                CallLevelUp();
            });
            learnMagicButton?.OnClickAddListener(() => 
            {
                CallLearnMagic();
            });
            learnMagicBackButton?.onClick.AddListener(() => 
            {
                CallHideLearnMagic();
            });
            SetLearnMagicButtonActive(false);
        }

        public void CallLevelUp()
        {
            if (levelUpButton.gameObject.activeSelf == false) return;
            //var eventData = new StatusViewEvent(CommandType.LevelUp);
            //_commandData(eventData);
        }

        public void CallLearnMagic()
        {
            if (learnMagicButton.gameObject.activeSelf == false) return;
            //var eventData = new StatusViewEvent(CommandType.ShowLearnMagic);
            //_commandData(eventData);
        }

        public void CallHideLearnMagic()
        {
            if (learnMagicBackButton.gameObject.activeSelf == false) return;
            //var eventData = new StatusViewEvent(CommandType.HideLearnMagic);
            //_commandData(eventData);
        }

        public void SetEvent(System.Action<ViewEvent> commandData)
        {
            //_commandData = commandData;
        }

        public void SetLearnMagicButtonActive(bool IsActive)
        {
            learnMagicBackButton?.gameObject.SetActive(IsActive);
        }

        public void SetActive(bool IsActive)
        {
            levelUpObj?.SetActive(IsActive);
        }

        public void SetLvUpCost(int cost)
        {
            lvUpCostText.SetText(cost + DataSystem.GetText(1000));
        }        
        
        public void ToLvText(int currentLv)
        {
            toLvText.SetText("Lv." + currentLv + "→" + (currentLv+1));
        }
    }
}
