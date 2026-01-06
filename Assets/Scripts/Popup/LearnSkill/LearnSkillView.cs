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
        [SerializeField] private ActorInfoComponent actorInfoComponent = null;
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
            evaluateObj?.SetActive(learnSkillInfo.From != learnSkillInfo.To);
            evaluateText?.SetText(DataSystem.GetReplaceDecimalText(learnSkillInfo.From.Value));
            afterEvaluateText?.SetText(DataSystem.GetReplaceDecimalText(learnSkillInfo.To.Value));
            skillInfoComponent.UpdateInfo(learnSkillInfo.SkillInfo);
            if (actorInfoComponent != null && learnSkillInfo.ActoInfo != null)
            {
                actorInfoComponent.UpdateInfo(learnSkillInfo.ActoInfo, null);
            }
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
        public ParameterInt From = new();
        public ParameterInt To = new();
        private ActorInfo _actoInfo;
        public ActorInfo ActoInfo => _actoInfo;
        private SkillInfo _skillInfo;
        public SkillInfo SkillInfo => _skillInfo;
        public LearnSkillInfo(int from, int to, SkillInfo skillInfo, ActorInfo actorInfo = null)
        {
            From.SetValue(from);
            To.SetValue(to);
            _skillInfo = skillInfo;
            _actoInfo = actorInfo;
        }

        public void SetToValue(int to)
        {
            To.SetValue(to);
        }

    }
}