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
        [SerializeField] private GridLayoutGroup gridLayoutGroup = null;

        public override void Initialize()
        {
            if (IsInitilized)
            {
                CallViewEvent(CommandType.Initialize);
                return;
            }
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
            mapCellList.SetInputHandler(InputKeyType.Cancel, () => CallViewEvent(CommandType.Back));
            mapCellList.SetInputHandler(InputKeyType.Decide, () => CallViewEvent(CommandType.DecideItem, mapCellList.ListItemData<MapCellInfo>()));
            AddViewActives(mapCellList);
        }

        public void SetDungeonMap(List<ListData> mapCellInfos, int constraintCount)
        {
            gridLayoutGroup.constraintCount = constraintCount;
            mapCellList.SetData(mapCellInfos, false, () =>
            {
            });
        }

        public void UpdateDungeonMap(List<ListData> mapCellInfos)
        {
            mapCellList.RefreshListData(mapCellInfos);
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
            Initialize,
            DecideItem,
            Back,
        }
    }
}
