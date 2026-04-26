using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FileList;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

namespace Ryneus
{
    public class FileListView : BaseView
    {
        [SerializeField] private BaseList fileList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        [SerializeField] private OnOffButton deleteButton = null;


        public override void Initialize()
        {
            if (IsInitilized)
            {
                CallViewEvent(CommandType.Initialize);
                return;
            }
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.FileList);
            SetBaseAnimation(popupAnimation);
            InitializeFileList();
            if (deleteButton != null)
            {
                deleteButton.OnClickAddListener(CallFileDelete);
            }
            _ = new FileListPresenter(this);
        }

        public new void SetBusy(bool busy)
        {
            SetActivate(busy ? null : fileList);
        }

        private void InitializeFileList()
        {
            fileList.Initialize();
            fileList.SetInputHandler(InputKeyType.Cancel, CommandEnd);
            fileList.SetInputHandler(InputKeyType.Decide, CallFileData);
            fileList.SetInputHandler(InputKeyType.Option1, CallFileDelete);
            AddViewActives(fileList);
        }

        public void OpenAnimation(Action initializeAfter)
        {
            popupAnimation.OpenAnimation(UiRoot.transform, initializeAfter);
        }

        public void SetFileList(List<ListData> fileDataList)
        {
            SetActivate(fileList);
            fileList.SetData(fileDataList);
        }

        private void CallFileData()
        {
            CallViewEvent(CommandType.DecideFile, fileList.ListItemData<SaveFileInfo>());
        }

        private void CallFileDelete()
        {
            CallViewEvent(CommandType.DeleteFile, fileList.ListItemData<SaveFileInfo>());
        }

        public void CommandEnd()
        {
            // 更新フラグを作るために位置初期化
            fileList.UpdateSelectIndex(0);
            BackEvent?.Invoke();
        }
    }
}

namespace FileList
{
    public enum CommandType
    {
        Initialize = 0,
        DecideFile = 1,
        DeleteFile,
    }
}