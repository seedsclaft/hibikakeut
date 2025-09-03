using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

namespace Ryneus
{
    public class ClassChangeView : BaseView, IInputHandlerEvent
    {
        [SerializeField] private ActorInfoComponent actorInfoComponent = null;
        [SerializeField] private StatusInfoComponent beforeStatusInfoComponent = null;
        [SerializeField] private ConfirmAnimation confirmAnimation = null;
        [SerializeField] private GameObject animRoot = null;
        [SerializeField] private GameObject animPrefab = null;

        private BattleStartAnim _battleStartAnim = null;
        private bool _animationBusy = false;

        private ClassChangeInfo _classChangeInfo;
        public override void Initialize()
        {
            base.Initialize();
            SetBaseAnimation(confirmAnimation);
            actorInfoComponent.gameObject.SetActive(false);
            StartLvUpAnimation();
        }

        public void OpenAnimation()
        {
            confirmAnimation.OpenAnimation(UiRoot.transform, null);
        }

        public void StartLvUpAnimation()
        {
            var prefab = Instantiate(animPrefab);
            prefab.transform.SetParent(animRoot.transform, false);
            _battleStartAnim = prefab.GetComponent<BattleStartAnim>();
            _battleStartAnim.gameObject.SetActive(false);
            _battleStartAnim.SetText("Class Change!");
            _battleStartAnim.StartAnim(false, 0, ShowLvUpActor);
            _battleStartAnim.gameObject.SetActive(true);
            _animationBusy = true;
        }

        public void ShowLvUpActor()
        {
            _animationBusy = false;
            actorInfoComponent.gameObject.SetActive(true);
            actorInfoComponent.Clear();
            actorInfoComponent.UpdateInfo(_classChangeInfo.ActorInfo, null);

            var rect = actorInfoComponent.gameObject.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(0, 0, 0);
            actorInfoComponent.MainThumb.DOFade(0, 0);

            BaseAnimation.MoveAndFade(rect, actorInfoComponent.MainThumb, 24, 1);

            //HelpWindow.SetInputInfo("LEVELUP");
            beforeStatusInfoComponent.UpdateInfo(_classChangeInfo.StatusInfo);
            //SetActivate(statusList);
        }

        public void SetClassChangeInfo(ClassChangeInfo classChangeInfo)
        {
            _classChangeInfo = classChangeInfo;
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
            if (_animationBusy)
            {
                return;
            }
            if (keyTypes.Count > 0 || InputSystem.IsMouseLeftButtonDown())
            {
                BackEvent?.Invoke();
            }
        }

    }

    public class ClassChangeInfo
    {
        private int _from = 0;
        public int From => _from;
        private int _to = 0;
        public int To => _to;
        private ActorInfo _actorInfo;
        public ActorInfo ActorInfo => _actorInfo;
        public StatusInfo StatusInfo;
        public ClassChangeInfo(ActorInfo skillInfo, StatusInfo statusInfo)
        {
            StatusInfo = statusInfo;
            _actorInfo = skillInfo;
        }
    }
}