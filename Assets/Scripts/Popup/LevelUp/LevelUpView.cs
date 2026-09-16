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
        [SerializeField] private PopupAnimation popupAnimation = null;
        [SerializeField] private BaseList statusList = null;
        [SerializeField] private ActorInfoComponent actorInfoComponent = null;
        [SerializeField] private BaseList learnSkillList = null;
        [SerializeField] private BattleStartAnim battleStartAnim = null;
        [SerializeField] private GameObject learnSkillRoot = null;
        [SerializeField] private TextMeshProUGUI beforeEvaluate = null;
        [SerializeField] private TextMeshProUGUI afterEvaluate = null;
        [SerializeField] private TextMeshProUGUI learnSkillText = null;
        private bool _busy = false;
        private bool _levelUpBusy = false;
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

        public void OpenAnimation()
        {
            popupAnimation.Initialize(UiRoot.transform);
        }

        public void LevelUpAnimation(string title, bool needLvUpAnimation)
        {
            _busy = false;
            UIComponent.SetActive(UiRoot, false);
            UIComponent.SetActive(learnSkillRoot, false);
            UIComponent.SetActive(actorInfoComponent.gameObject, false);
            UIComponent.SetActive(statusList.gameObject.gameObject, false);
            var rect = actorInfoComponent.gameObject.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(-240, rect.localPosition.y, 0);
            actorInfoComponent.MainThumb.DOFade(0, 0);
            //SetBusy(true);
            battleStartAnim.SetText(title);
            ChangeBackCommandActive(false);
            if (needLvUpAnimation)
            {
                _levelUpBusy = true;
                battleStartAnim.StartAnim(false, 0, () =>
                {
                    if (!_levelUpBusy)
                    {
                        return;
                    }
                    _levelUpBusy = false;
                    OpenPopup();
                });
            } else
            {
                OpenPopup();
            }
            UIComponent.SetActive(battleStartAnim?.gameObject, true);
        }

        private void OpenPopup()
        {
            popupAnimation.OpenAnimation(UiRoot.transform, () =>
            {
                CallViewEvent(CommandType.EndAnimation);
                UIComponent.SetActive(UiRoot, true);
                //SetBusy(false);
                ChangeBackCommandActive(true);
            });
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
                UIComponent.SetActive(actorInfoComponent?.gameObject, true);
                var rect = actorInfoComponent.gameObject.GetComponent<RectTransform>();
                BaseAnimation.MoveAndFade(rect, actorInfoComponent.MainThumb, -280, 1, 0.5f);
            }
            if (statusList != null)
            {
                UIComponent.SetActive(statusList?.gameObject, true);
                statusList.SetData(statusDates);
                SetActivate(statusList);
            }
        }

        public void UpdateLearnSkill(ActorInfo actorInfo, List<SkillInfo> skillInfos)
        {
            if (actorInfoComponent != null)
            {
                actorInfoComponent.UpdateInfo(actorInfo, null);
                UIComponent.SetActive(actorInfoComponent?.gameObject, true);
                var rect = actorInfoComponent.gameObject.GetComponent<RectTransform>();
                BaseAnimation.MoveAndFade(rect, actorInfoComponent.MainThumb, -280, 1, 0.5f);
            }
            if (learnSkillList != null)
            {
                UIComponent.SetActive(learnSkillRoot, true);
                learnSkillList.SetData(ListData.MakeListData(skillInfos));
                UIComponent.SetActive(learnSkillList.gameObject, true);
            }
        }

        public void UpdateLearnSkillText(string text)
        {
            UIComponent.SetText(learnSkillText, text);
        }

        public void UpdateEvaluate(int from, int to)
        {
            UIComponent.SetText(beforeEvaluate, from.ToString());
            UIComponent.SetText(afterEvaluate, to.ToString());
        }

        public void ClearActorThumb()
        {
            actorInfoComponent.Clear();
        }

        private void CallLevelUpNext()
        {
            CallViewEvent(CommandType.LevelUpNext);
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (keyTypes.Contains(InputKeyType.Decide) && _levelUpBusy)
            {
                battleStartAnim.Reset();
                _levelUpBusy = false;
                OpenPopup();
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
            if (keyTypes.Contains(InputKeyType.Decide))
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