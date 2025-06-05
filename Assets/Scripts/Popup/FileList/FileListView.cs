using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FileList;

namespace Ryneus
{
    public class FileListView : BaseView
    {
        [SerializeField] private BaseList fileList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;


        public override void Initialize()

        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.FileList);
            SetBaseAnimation(popupAnimation);
            InitializeFileList();
            _ = new FileListPresenter(this);
        }

        private void InitializeFileList()
        {
            fileList.Initialize();
            fileList.SetInputHandler(InputKeyType.Cancel, () => BackEvent?.Invoke());
            fileList.SetInputHandler(InputKeyType.Decide, CallFileData);
            AddViewActives(fileList);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform, null);
        }

        public void SetFileList(List<ListData> fileDataList)
        {
            SetActivate(fileList);
            fileList.SetData(fileDataList);
        }

        private void CallFileData()
        {
            var listData = fileList.ListData;
            if (listData != null)
            {
                var data = (SaveFileInfo)listData.Data;
                CallViewEvent(CommandType.DecideFile, data);
            }
        }

        public void CommandEnd()
        {
            BackEvent?.Invoke();
        }
    }
}

namespace FileList
{
    public enum CommandType
    {
        DecideFile = 1,
    }
}