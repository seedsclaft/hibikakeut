using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Ryneus.Dungeon;
using UnityEngine;

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

        private async Task Initialize(float timeStamp = 0)
        {
            //_view.SetHelpWindow();
            _view.SetEvent((type) => UpdateCommand(type));

            _view.SetPartyUnitList(MakeListData(_model.PartyUnit(),-1));
            await PlayTacticsBgm(timeStamp);
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
            _model.CommandMoveEnd();

            CommandRefresh();

            // ターン数が0の場合
            if (_model.EndDungeonByTurnCount())
            {
                var confirmInfo = new ConfirmInfo("残りターン数が枯渇したため帰還します!",(a) => 
                {
                    _view.CommandSceneChange(Scene.MainMenu);
                });
                _view.CommandCallConfirm(confirmInfo);
                return;
            }

            // エンカウントした場合
            if (_model.EncountEnemy())
            {
                _model.ResetEncountValue();
                var battleSceneInfo = new BattleSceneInfo
                {
                    ActorInfos = _model.PartyInfo.CurrentDeckActorInfos(),
                    EnemyInfos = _model.RandumTroopInfos(),
                };
                PlayBattleBgm();
                _view.CommandChangeViewToTransition(null);
                _view.ChangeUIActive(false);
                _view.CommandSceneChange(Scene.Battle, battleSceneInfo);
                SoundManager.Instance.PlayStaticSe(SEType.BattleStart);
            }
        }

        private void CommandSelectSideMenu()
        {
            _busy = true;
            _view.CallSystemCommand(Base.CommandType.DungeonBusy,true);
            CommandCallSideMenu(MakeListData(_model.SideMenu()), () =>
            {
                _view.CallSystemCommand(Base.CommandType.DungeonBusy,false);
                _busy = false;
            });
        }

        private void CommandRefresh()
        {
            _view.CommandRefresh();
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
        }


    }
}