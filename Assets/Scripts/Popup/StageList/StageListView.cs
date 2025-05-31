using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ryneus.StageList;

namespace Ryneus
{
    public class StageListView : BaseView
    {
        [SerializeField] private BaseList stageList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.StageList);
            InitializeStageList();
            SetBaseAnimation(popupAnimation);
            _ = new StageListPresenter(this);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform, () => {});
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
        }
    }

    namespace StageList
    {
        public enum CommandType
        {
            DecideStage = 0,
        }
    }
}
