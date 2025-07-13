using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class ReleaseListView : BaseView
    {
        [SerializeField] private PartyInfoComponent partyInfoComponent;
        [SerializeField] private BaseList releaseList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;


        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.ReleaseList);
            InitializeReleaseList();
            SetBaseAnimation(popupAnimation);
            _ = new ReleaseListPresenter(this);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform, () => CallViewEvent(ReleaseList.CommandType.EndOpenAnimation));
        }

        private void InitializeReleaseList()
        {
            releaseList.Initialize();
            releaseList.SetInputHandler(InputKeyType.Cancel, () => BackEvent());
            releaseList.SetInputHandler(InputKeyType.Decide, () => CallViewEvent(ReleaseList.CommandType.DecideBuilding, releaseList.ListItemData<BuildingInfo>()));
            AddViewActives(releaseList);
        }

        public void SetBuildingList(List<ListData> characterLists)
        {
            releaseList.SetData(characterLists);
            releaseList.Activate();
        }

        public void CommandRefresh()
        {
            partyInfoComponent.UpdateCurrentInfo();
        }
    }

    namespace ReleaseList
    {
        public enum CommandType
        {
            None = 0,
            DecideBuilding = 1,
            EndOpenAnimation = 2,
        }
    }
}
