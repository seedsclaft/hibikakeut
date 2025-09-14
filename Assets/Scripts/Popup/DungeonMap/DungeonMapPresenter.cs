using System;
using System.Collections;
using System.Collections.Generic;
using Ryneus.DungeonMap;
using UnityEditor.Recorder;

namespace Ryneus
{
    public class DungeonMapPresenter : BasePresenter
    {
        DungeonMapModel _model = null;
        DungeonMapView _view = null;

        private bool _busy = true;
        public DungeonMapPresenter(DungeonMapView view)
        {
            _view = view;
            _model = new DungeonMapModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetDungeonMap(MakeListData(_model.MapCellInfos(), 0));
            _view.OpenAnimation();
            _view.ActivateDungeonMap(true);
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.DungeonMap)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.DecideItem:
                    CommandDecideItem((MapCellInfo)viewEvent.Template);
                    break;
            }
        }

        private void CommandDecideItem(MapCellInfo mapInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            LogOutput.Log(mapInfo.MapInfo.eventId);
            return;
            _view.ActivateDungeonMap(false);
            _busy = true;
            var confirmInfo = new ConfirmInfo(DataSystem.GetText(34040), (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                }
                _busy = false;
                _view.ActivateDungeonMap(true);
            });
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}