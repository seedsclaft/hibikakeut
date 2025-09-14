using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ryneus.DungeonMap;

namespace Ryneus
{
    public class DungeonMapView : BaseView
    {
        [SerializeField] private BaseList mapCellList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.DungeonMap);
            InitializeDungeonMap();
            SetBaseAnimation(popupAnimation);
            _ = new DungeonMapPresenter(this);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform, () => {});
        }

        private void InitializeDungeonMap()
        {
            mapCellList.Initialize();
            mapCellList.SetInputHandler(InputKeyType.Cancel, () => BackEvent());
            mapCellList.SetInputHandler(InputKeyType.Decide, () => CallViewEvent(CommandType.DecideItem, mapCellList.ListItemData<MapCellInfo>()));
            AddViewActives(mapCellList);
        }

        public void SetDungeonMap(List<ListData> achievementLists)
        {
            mapCellList.SetData(achievementLists, false, () =>
            {
            });
        }

        public void UpdateDungeonMap(List<ListData> achievementLists)
        {
            mapCellList.RefreshListData(achievementLists);
            mapCellList.UpdateAllItems();
        }

        public void CheckItemDetailButtonActive()
        {
        }

        public void ActivateDungeonMap(bool isActivate)
        {
            SetActivate(isActivate ? mapCellList : null);
        }
    }

    namespace DungeonMap
    {
        public enum CommandType
        {
            DecideItem = 0,
            DetailItem,
        }
    }
}
