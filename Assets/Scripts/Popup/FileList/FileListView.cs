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
        private new Action<ViewEvent> _commandData = null;
        public new void SetEvent(Action<ViewEvent> commandData)
        {
            _commandData = commandData;
        }
        public void CallEvent(CommandType battleCommandType,object sendData = null)
        {
            var commandType = new ViewCommandType(ViewCommandSceneType.BattleParty,battleCommandType);
            var eventData = new ViewEvent(commandType)
            {
                template = sendData
            };
            _commandData(eventData);
        }
        
        public override void Initialize() 
        {
            base.Initialize();
            SetBaseAnimation(popupAnimation);
            InitializeFileList();
            new FileListPresenter(this);
        }

        private void InitializeFileList()
        {
            fileList.Initialize();
            fileList.SetInputHandler(InputKeyType.Cancel,BackEvent);
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
            fileList.SetData(fileDataList);
            SetActivate(fileList);
        }

        private void CallFileData()
        {
            var listData = fileList.ListData;
            if (listData != null)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var data = (SaveFileInfo)listData.Data;
                CallEvent(CommandType.DecideFile,data);
            }
        }

        public void CommandEnd()
        {
            BackEvent();
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