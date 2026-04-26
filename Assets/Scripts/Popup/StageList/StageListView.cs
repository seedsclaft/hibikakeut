using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ryneus.StageList;
using System;

namespace Ryneus
{
    public class StageListView : BaseView
    {
        [SerializeField] private BaseList stageList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        [SerializeField] private PartyInfoComponent partyInfoComponent;

        public override void Initialize()
        {
            if (IsInitilized)
            {
                CallViewEvent(CommandType.Initialize);
                return;
            }
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.StageList);
            InitializeStageList();
            SetBaseAnimation(popupAnimation);
            _ = new StageListPresenter(this);
        }

        public void OpenAnimation(Action initializeAfter)
        {
            popupAnimation.OpenAnimation(UiRoot.transform, initializeAfter);
        }

        private void InitializeStageList()
        {
            stageList.Initialize();
            stageList.SetInputHandler(InputKeyType.Cancel, () => BackEvent());
            stageList.SetInputHandler(InputKeyType.Decide, () => CallViewEvent(CommandType.DecideStage, stageList.ListItemData<StageInfo>()));
            SetInputHandler(stageList.gameObject);
        }

        public void SetStageList(List<ListData> achievementLists)
        {
            stageList.SetData(achievementLists);
            stageList.Activate();
            partyInfoComponent.UpdateCurrentInfo();
        }
    }

    namespace StageList
    {
        public enum CommandType
        {
            Initialize,
            DecideStage,
        }
    }
}
