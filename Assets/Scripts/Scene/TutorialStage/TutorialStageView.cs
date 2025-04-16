using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TutorialStage;

namespace Ryneus
{
    public class TutorialStageView : BaseView
    {
        [SerializeField] private BaseList stageList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        
        public override void Initialize() 
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.TutorialStage);
            SetBaseAnimation(popupAnimation);
            InitializeTutorialStage();
            new TutorialStagePresenter(this);
        }

        private void InitializeTutorialStage()
        {
            stageList.Initialize();
            stageList.SetInputHandler(InputKeyType.Cancel,() => BackEvent?.Invoke());
            stageList.SetInputHandler(InputKeyType.Decide,CallStageData);
            SetInputHandler(stageList.gameObject);
            AddViewActives(stageList);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform,null);
        }

        public void SetTutorialStage(List<ListData> stageDataList)
        {
            SetActivate(stageList);
            stageList.SetData(stageDataList);
        }

        public void ActivateStageList()
        {
            SetActivate(stageList);
        }

        public void DeactivateStageList()
        {
            SetActivate(null);
        }

        private void CallStageData()
        {
            var listData = stageList.ListItemData<StageInfo>();
            if (listData != null)
            {
                CallViewEvent(CommandType.DecideStage,listData);
            }
        }

        public void CommandEnd()
        {
            BackEvent?.Invoke();
        }
    }
}

namespace TutorialStage
{
    public enum CommandType
    {
        DecideStage = 1,
    }
}