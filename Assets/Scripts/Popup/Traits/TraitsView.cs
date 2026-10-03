using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ryneus.Traits;
using System;

namespace Ryneus
{
    public class TraitsView : BaseView
    {
        [SerializeField] private BaseList traitsList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;

        public override void Initialize()
        {
            if (IsInitilized)
            {
                CallViewEvent(CommandType.Initialize);
                return;
            }
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Traits);
            InitializeTraits();
            SetBaseAnimation(popupAnimation);
            _ = new TraitsPresenter(this);
        }

        public void OpenAnimation(Action initializeAfter)
        {
            popupAnimation.OpenAnimation(UiRoot.transform, () => 
            {
                initializeAfter?.Invoke();
                CallViewEvent(CommandType.EndOpenAnimation);
            });
        }

        private void InitializeTraits()
        {
            traitsList.Initialize();
            traitsList.SetInputHandler(InputKeyType.Cancel, () => BackEvent());
            AddViewActives(traitsList);
        }

        public void SetTraitsList(List<ListData> characterLists)
        {
            traitsList.SetData(characterLists);
            traitsList.Activate();
        }
    }

    namespace Traits
    {
        public enum CommandType
        {
            Initialize = 0,
            EndOpenAnimation,
        }
    }
}
