using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Ryneus.Dungeon;

namespace Ryneus
{
    public class DungeonPresenter : BasePresenter
    {
        DungeonModel _model = null;
        DungeonView _view = null;

        private bool _busy = true;
        public DungeonPresenter(DungeonView view)
        {
            _view = view;
            SetView(_view);
            _model = new DungeonModel();
            SetModel(_model);

            Initialize();
        }

        private async Task Initialize()
        {
            //_view.SetHelpWindow();
            _view.SetEvent((type) => UpdateCommand(type));

            _view.SetPartyUnitList(MakeListData(_model.PartyUnit(),-1));
            // ダンジョン生成
            _view.CommandChangeDungeon("DefaultDungeon");
            var m = GameSystem.Instance.MoveController;
            _busy = false;
        }


        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Dungeon)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.MoveEnd:
                    CommandMoveEnd();
                    break;
                case CommandType.SelectSideMenu:
                    CommandSelectSideMenu();
                    break;
            }
        }

        private void CommandMoveEnd()
        {
            var battleSceneInfo = new BattleSceneInfo
            {
                ActorUnitInfos = _model.PartyUnit(),
                EnemyUnitInfos = new List<UnitInfo>(){_model.RandumTroopInfo()},
            };
            _view.CommandChangeViewToTransition(null);
            _view.ChangeUIActive(false);
            _view.CommandSceneChange(Scene.Battle, battleSceneInfo);
        }

        private void CommandSelectSideMenu()
        {
            _busy = true;
            CommandCallSideMenu(MakeListData(_model.SideMenu()), () =>
            {
                _busy = false;
            });
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
        }


    }
}