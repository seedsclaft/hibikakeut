using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ryneus.SideMenu;

namespace Ryneus
{
    public class SideMenuView : BaseView
    {
        [SerializeField] private BaseList sideMenuInfoList = null;
        [SerializeField] private Button closeButton = null;
        [SerializeField] private SideMenuAnimation sideMenuAnimation = null;

        public override void Initialize()
        {
            if (IsInitilized)
            {
                CallViewEvent(CommandType.Initialize);
                return;
            }
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.SideMenu);
            InitializeSideMenuInfo();
            sideMenuInfoList.Initialize();
            closeButton.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                BackEvent();
            });
            SetBaseAnimation(sideMenuAnimation);
            _ = new SideMenuPresenter(this);
        }

        public void OpenAnimation()
        {
            sideMenuAnimation?.OpenAnimation(UiRoot.transform, null);
        }

        private void OnClickSideMenu()
        {
            CallViewEvent(CommandType.SelectSideMenu,sideMenuInfoList.ListItemData<SystemData.CommandData>());
        }

        private void InitializeSideMenuInfo()
        {
            sideMenuInfoList.SetInputHandler(InputKeyType.Decide, () =>
            {
                sideMenuInfoList.Deactivate();
                OnClickSideMenu();
            });
            sideMenuInfoList.SetInputHandler(InputKeyType.Cancel, () =>
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                BackEvent();
            });
            SetInputHandler(sideMenuInfoList.gameObject);
        }

        public void SetSideMenuViewInfo(SideMenuViewInfo sideMenuViewInfo)
        {
            sideMenuInfoList.SetData(sideMenuViewInfo.CommandLists);
        }

        public void ActivateSideMenu()
        {
            sideMenuInfoList.Activate();
        }
    }

    public class SideMenuViewInfo
    {
        public List<ListData> CommandLists;
    }

    namespace SideMenu
    {
        public enum CommandType
        {
            Initialize,
            SelectSideMenu,
        }
    }
}