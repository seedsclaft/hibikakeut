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
        [SerializeField] private BaseList learnSkillList = null;
        [SerializeField] private ConfirmAnimation confirmAnimation = null;
        public override void Initialize()
        {
            base.Initialize();
            SetBaseAnimation(confirmAnimation);
            InitializeSkillList();
            OpenAnimation();
        }

        private void InitializeSkillList()
        {
            learnSkillList.Initialize();
            AddViewActives(learnSkillList);
        }

        public void OpenAnimation()
        {
            confirmAnimation.OpenAnimation(UiRoot.transform, null);
        }

        public void SetLearnSkillInfo(LearnSkillInfo learnSkillInfo)
        {
            UIComponent.SetActive(evaluateObj, learnSkillInfo.From != learnSkillInfo.To);
            UIComponent.SetText(evaluateText, DataSystem.GetReplaceDecimalText(learnSkillInfo.From.Value));
            UIComponent.SetText(afterEvaluateText, DataSystem.GetReplaceDecimalText(learnSkillInfo.To.Value));
            if (learnSkillInfo != null && learnSkillInfo.SkillInfos.Count > 0)
            {
                learnSkillList.SetData(ListData.MakeListData(learnSkillInfo.SkillInfos));
            }
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (Busy.Value)
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
        private List<SkillInfo> _skillInfos;
        public List<SkillInfo> SkillInfos => _skillInfos;
        public LearnSkillInfo(int from, int to, List<SkillInfo> skillInfos, ActorInfo actorInfo = null)
        {
            From.SetValue(from);
            To.SetValue(to);
            _skillInfos = skillInfos;
            _actorInfo = actorInfo;
        }

        public void SetToValue(int to)
        {
            To.SetValue(to);
        }

    }
}