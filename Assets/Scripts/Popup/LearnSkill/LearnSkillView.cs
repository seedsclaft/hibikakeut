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
            confirmAnimation.OpenAnimation(UiRoot.transform, null);
        }

        public void SetLearnSkillInfo(LearnSkillInfo learnSkillInfo)
        {
            if (evaluateObj != null)
            {
                evaluateObj.SetActive(learnSkillInfo.From != learnSkillInfo.To);
            }
            if (evaluateText != null)
            {
                evaluateText.SetText(DataSystem.GetReplaceDecimalText(learnSkillInfo.From.Value));
            }
            if (afterEvaluateText != null)
            {
                afterEvaluateText.SetText(DataSystem.GetReplaceDecimalText(learnSkillInfo.To.Value));
            }
            if (skillInfoComponent != null && learnSkillInfo.SkillInfo != null)
            {
                skillInfoComponent.UpdateInfo(learnSkillInfo.SkillInfo);
            }
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (Busy)
            {
                return;
            }
            if (keyTypes.Count > 0)
            {
                BackEvent?.Invoke();
            }
        }

    }

    public class LearnSkillInfo
    {
        public ParameterInt From = new();
        public ParameterInt To = new();
        public ParameterString Title = new();
        private ActorInfo _actorInfo;
        public ActorInfo ActorInfo => _actorInfo;
        private SkillInfo _skillInfo;
        public SkillInfo SkillInfo => _skillInfo;
        public LearnSkillInfo(int from, int to, SkillInfo skillInfo, ActorInfo actorInfo = null)
        {
            From.SetValue(from);
            To.SetValue(to);
            _skillInfo = skillInfo;
            _actorInfo = actorInfo;
        }

        public void SetToValue(int to)
        {
            To.SetValue(to);
        }

    }
}