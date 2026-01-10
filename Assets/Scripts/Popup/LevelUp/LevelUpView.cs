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
        [SerializeField] private SkillInfoComponent skillInfoComponent = null;
        [SerializeField] private BattleStartAnim battleStartAnim = null;
        [SerializeField] private GameObject learnSkillRoot = null;
        [SerializeField] private TextMeshProUGUI beforeEvaluate = null;
        [SerializeField] private TextMeshProUGUI afterEvaluate = null;
        [SerializeField] private TextMeshProUGUI learnSkillText = null;
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
            _ = new LevelUpPresenter(this);
        }

        public void OpenAnimation(string title)
        {
            UiRoot.SetActive(false);
            learnSkillRoot.SetActive(false);
            actorInfoComponent.gameObject.SetActive(false);
            skillInfoComponent.gameObject.SetActive(false);
            statusList.gameObject.SetActive(false);
            var rect = actorInfoComponent.gameObject.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(-240, rect.localPosition.y, 0);
            actorInfoComponent.MainThumb.DOFade(0, 0);
            SetBusy(true);
            battleStartAnim.SetText(title);
            battleStartAnim.StartAnim(false, 0, () =>
            {
                confirmAnimation.OpenAnimation(UiRoot.transform, () =>
                {
                    CallViewEvent(CommandType.EndAnimation);
                    UiRoot.SetActive(true);
                    SetBusy(false);
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

        public void UpdateLearnSkill(ActorInfo actorInfo, SkillInfo skillInfo)
        {
            if (actorInfoComponent != null)
            {
                actorInfoComponent.UpdateInfo(actorInfo, null);
                actorInfoComponent.gameObject.SetActive(true);
            }
            if (skillInfoComponent != null)
            {
                learnSkillRoot.SetActive(true);
                skillInfoComponent.UpdateInfo(skillInfo);
                skillInfoComponent.gameObject.SetActive(true);
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
            if (keyTypes.Count > 0)
            {
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