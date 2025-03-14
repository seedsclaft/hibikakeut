using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class LearnSkillView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private GameObject evaluateObj = null;
        [SerializeField] private TextMeshProUGUI evaluateText = null;
        [SerializeField] private TextMeshProUGUI afterEvaluateText = null;
        [SerializeField] private SkillInfoComponent skillInfoComponent = null;
        [SerializeField] private ConfirmAnimation confirmAnimation = null;

        public override void Initialize() 
        {
            base.Initialize();
            SetBaseAnimation(confirmAnimation);
            OpenAnimation();
        }

        public void OpenAnimation()
        {
            confirmAnimation.OpenAnimation(UiRoot.transform,null);
        }

        public void SetLearnSkillInfo(LearnSkillInfo learnSkillInfo)
        {
            evaluateObj?.SetActive(learnSkillInfo.From !=  learnSkillInfo.To);
            evaluateText?.SetText(learnSkillInfo.From.ToString());
            afterEvaluateText?.SetText(learnSkillInfo.To.ToString());
            skillInfoComponent.UpdateInfo(learnSkillInfo.SkillInfo);
        }
        
        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (keyTypes.Count > 0)
            {
                BackEvent?.Invoke();
            }
        }

    }

    public class LearnSkillInfo
    {
        private int _from = 0;
        public int From => _from;
        private int _to = 0;
        public int To => _to;
        private SkillInfo _skillInfo;
        public SkillInfo SkillInfo => _skillInfo;
        public LearnSkillInfo(int from,int to,SkillInfo skillInfo)
        {
            _from = from;
            _to = to;
            _skillInfo = skillInfo;
        }

        public void SetToValue(int to)
        {
            _to = to;
        }

    }
}