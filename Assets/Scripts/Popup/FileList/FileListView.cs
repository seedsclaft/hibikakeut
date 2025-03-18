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
            new FileListPresenter(this);
        }

        private void InitializeFileList()
        {
            fileList.Initialize();
            fileList.SetInputHandler(InputKeyType.Cancel,() => BackEvent?.Invoke());
            fileList.SetInputHandler(InputKeyType.Decide,CallFileData);
            SetInputHandler(fileList.gameObject);
            AddViewActives(fileList);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform,null);
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
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var data = (SaveFileInfo)listData.Data;
                CallViewEvent(CommandType.DecideFile,data);
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