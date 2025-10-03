using System;
using System.Collections;
using System.Collections.Generic;
using Ryneus.DungeonMap;

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

            SetView(_view);
            _view.SetEvent((type) => UpdateCommand(type));
            Initialize();
        }

        private void Initialize()
        {
            _model = new DungeonMapModel();
            SetModel(_model);
            _view.SetDungeonMap(MakeListData(_model.MapCellInfos(), 0), _model.ConstraintCount());
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
                case CommandType.Initialize:
                    Initialize();
                    break;
                case CommandType.DecideItem:
                    CommandDecideItem((MapCellInfo)viewEvent.Template);
                    break;
                case CommandType.Back:
                    CommandBack();
                    break;
            }
        }

        private void CommandDecideItem(MapCellInfo mapInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.FindPath(mapInfo);
            _view.UpdateDungeonMap(MakeListData(_model.MapCellInfos(), 0));
        }

        private void CommandBack()
        {
            _model.SetRoutePaths();
            _view.BackEvent();
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}