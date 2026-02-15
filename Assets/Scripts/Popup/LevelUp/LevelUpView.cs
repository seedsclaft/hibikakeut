using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ryneus.LevelUp;
using TMPro;
using DG.Tweening;

namespace Ryneus
{
    public class LevelUpView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private ConfirmAnimation confirmAnimation = null;
        [SerializeField] private BaseList statusList = null;
        [SerializeField] private ActorInfoComponent actorInfoComponent = null;
        [SerializeField] private BaseList learnSkillList = null;
        [SerializeField] private BattleStartAnim battleStartAnim = null;
        [SerializeField] private GameObject learnSkillRoot = null;
        [SerializeField] private TextMeshProUGUI beforeEvaluate = null;
        [SerializeField] private TextMeshProUGUI afterEvaluate = null;
        [SerializeField] private TextMeshProUGUI learnSkillText = null;
        private bool _busy = false;
        public override void Initialize()
        {
            if (IsInitilized)
            {
                CallViewEvent(CommandType.Initialize);
                return;
            }
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.LevelUp);
            InitializeStatusList();
            InitializeSkillList();
            _ = new LevelUpPresenter(this);
        }

        public void OpenAnimation(string title)
        {
            _busy = false;
            UiRoot.SetActive(false);
            learnSkillRoot.SetActive(false);
            actorInfoComponent.gameObject.SetActive(false);
            statusList.gameObject.SetActive(false);
            var rect = actorInfoComponent.gameObject.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(-240, rect.localPosition.y, 0);
            actorInfoComponent.MainThumb.DOFade(0, 0);
            SetBusy(true);
            battleStartAnim.SetText(title);
            ChangeBackCommandActive(false);
            battleStartAnim.StartAnim(false, 0, () =>
            {
                confirmAnimation.OpenAnimation(UiRoot.transform, () =>
                {
                    CallViewEvent(CommandType.EndAnimation);
                    UiRoot.SetActive(true);
                    SetBusy(false);
                    ChangeBackCommandActive(true);
                });
            });
            battleStartAnim.gameObject.SetActive(true);
        }

        private void InitializeStatusList()
        {
            statusList.Initialize();
            statusList.SetInputHandler(InputKeyType.Decide, CallLevelUpNext);
            AddViewActives(statusList);
        }
        private void InitializeSkillList()
        {
            learnSkillList.Initialize();
            AddViewActives(learnSkillList);
        }

        public void UpdateLevelUp(ActorInfo actorInfo, List<ListData> statusDates)
        {
            if (actorInfoComponent != null)
            {
                actorInfoComponent.UpdateInfo(actorInfo, null);
                actorInfoComponent.gameObject.SetActive(true);
                var rect = actorInfoComponent.gameObject.GetComponent<RectTransform>();
                BaseAnimation.MoveAndFade(rect, actorInfoComponent.MainThumb, -280, 1, 0.5f);

            }
            if (statusList != null)
            {
                statusList.gameObject.SetActive(true);
                statusList.SetData(statusDates);
                SetActivate(statusList);
            }
        }

        public void UpdateLearnSkill(ActorInfo actorInfo, List<SkillInfo> skillInfos)
        {
            if (actorInfoComponent != null)
            {
                actorInfoComponent.UpdateInfo(actorInfo, null);
                actorInfoComponent.gameObject.SetActive(true);
                var rect = actorInfoComponent.gameObject.GetComponent<RectTransform>();
                BaseAnimation.MoveAndFade(rect, actorInfoComponent.MainThumb, -280, 1, 0.5f);
            }
            if (learnSkillList != null)
            {
                learnSkillRoot.SetActive(true);
                learnSkillList.SetData(ListData.MakeListData(skillInfos));
                learnSkillList.gameObject.SetActive(true);
            }
        }

        public void UpdateLearnSkillText(string text)
        {
            learnSkillText.SetText(text);
        }

        public void UpdateEvaluate(int from, int to)
        {
            beforeEvaluate.SetText(from.ToString());
            afterEvaluate.SetText(to.ToString());
        }

        private void CallLevelUpNext()
        {
            CallViewEvent(CommandType.LevelUpNext);
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (Busy)
            {
                return;
            }
            if (_busy)
            {
                return;
            }
            if (statusList.gameObject.activeSelf)
            {
                return;
            }
            if (keyTypes.Count > 0)
            {
                _busy = true;
                CallLevelUpNext();
            }
        }

    }

    namespace LevelUp
    {
        public enum CommandType
        {
            Initialize,
            EndAnimation,
            LevelUpNext,
        }
    }
}