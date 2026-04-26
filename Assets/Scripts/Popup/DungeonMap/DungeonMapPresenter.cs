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
            Initialize(true);
        }

        private void Initialize(bool first)
        {
            _model = new DungeonMapModel();
            SetModel(_model);
            _view.OpenAnimation(first ? InitializeAfter : null);
            if (!first)
            {
                InitializeAfter();
            }
        }

        private void InitializeAfter()
        {
            _view.SetDungeonMap(MakeListData(_model.MapCellInfos(), 0), _model.ConstraintCount());
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
                    Initialize(false);
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