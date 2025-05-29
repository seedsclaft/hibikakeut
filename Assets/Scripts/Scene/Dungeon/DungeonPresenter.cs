using System;
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

            // イベントマスの場合
            if (CheckEventData(() => {_view.CallSystemCommand(Base.CommandType.DungeonBusy,false);}))
            {
                _view.CallSystemCommand(Base.CommandType.DungeonBusy,true);
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

        private bool CheckEventData(Action endEvent = null)
        {
            var playerPosition = Ariadne.PlayerPosition.Instance.playerPos;
            UnityEngine.Debug.Log(playerPosition);
            var stageEvent = GetStageEventData(EventTiming.Dungeon,playerPosition.x,playerPosition.y);
            if (stageEvent != null)
            {
                switch (stageEvent.Type)
                {
                    case StageEventType.AdvStart:
                        // TimeStampを取得してBgmをフェードアウト
                        var timeStamp = SoundManager.Instance.CurrentTimeStamp();
                        if (CheckAdvEvent(EventTiming.BattleVictory,timeStamp,() => CheckEventData(() => Initialize())))
                        {
                            return true;
                        }
                        return true;
                    case StageEventType.ExitDungeon:
                        CommandReturn();
                        return true;
                    case StageEventType.SelectAddActor:
                        // 選択して仲間を加入
                        // 確認後仲間選択
                        var confirmInfo = new ConfirmInfo("加入したい仲間を選択！",(a) =>
                        {
                            CommandCallAddActorInfo(true,true);
                        });
                        confirmInfo.SetIsNoChoice(true);
                        confirmInfo.SetBackEvent(() => {});
                        _view.CommandCallConfirm(confirmInfo);
                        _model.AddEventReadFlag(stageEvent);
                        SoundManager.Instance.PlayStaticSe(SEType.Decide);
                        return true;
                    case StageEventType.ForceBattle:
                        _model.AddEventReadFlag(stageEvent);
                        var battleSceneInfo = new BattleSceneInfo
                        {
                            ActorInfos = _model.PartyInfo.CurrentDeckActorInfos(),
                            EnemyInfos = _model.ForceBattleTroopInfos(stageEvent.Param),
                        };
                        PlayBattleBgm();
                        _view.CommandChangeViewToTransition(null);
                        _view.ChangeUIActive(false);
                        _view.CommandSceneChange(Scene.Battle, battleSceneInfo);
                        SoundManager.Instance.PlayStaticSe(SEType.BattleStart);
                        return true;
                }
            }
            endEvent?.Invoke();
            return false;
        }

        private void CommandReturn()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo("帰還しますか？", (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    _view.CallSystemCommand(Base.CommandType.ClosePopupAll);
                    _view.CommandSceneChange(Scene.MainMenu);
                }
            });
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CommandCallAddActorInfo(bool freeSelect,bool addCommand)
        {
            List<ActorInfo> actorInfos = null;
            if (freeSelect)
            {
                actorInfos = _model.AddSelectActorInfos();
            } else
            {
                //actorInfos = _model.AddSelectActorGetItemInfos(symbolResultInfo.SymbolInfo.GetItemInfos);
            }
            if (addCommand)
            {
                // 加入する用
                CommandAddActorStatusInfo(actorInfos,() =>
                {

                });
            } else
            {
                /*
                var selectActorId = -1;
                var getItemInfo = _view.SymbolGetItemInfo;
                if (getItemInfo != null)
                {
                    selectActorId = getItemInfo.Param1;
                }
                // 確認する用
                CommandStatusInfo(actorInfos,false,true,false,false,selectActorId,() => 
                {

                });
                */
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