using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ryneus.MainMenu;

namespace Ryneus
{
    public class MainMenuPresenter : BasePresenter
    {
        MainMenuModel _model = null;
        MainMenuView _view = null;

        private bool _busy = true;
        private CommandType _backCommand = CommandType.None;
        public MainMenuPresenter(MainMenuView view)
        {
            _view = view;
            SetView(_view);
            _model = new MainMenuModel();
            SetModel(_model);

            Initialize();
        }

        private async void Initialize()
        {
            _view.SetHelpWindow();
            _view.SetEvent((type) => UpdateCommand(type));
            if (_model.IsEnding())
            {
                // エンディング再生
                var advInfo = new AdvCallInfo();
                advInfo.Label.SetValue(_model.GetAdvFile(101));
                advInfo.SetCallEvent(() => 
                {
                    _busy = false;
                    _view.ChangeUIActive(true);
                    _view.CommandGotoSceneChange(Scene.Result);
                });
                _view.CommandCallAdv(advInfo);
                _view.ChangeUIActive(false);
                return;
            }

            _view.SetCharaLayer(_model.PartyInfo.CurrentDeckActorInfos());
            _view.SetCommandList(_model.MainMenuCommand());

            var bgm = await _model.GetBgmData("Mainmenu");
            SoundManager.Instance.PlayBgm(bgm,1.0f,true);
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.MainMenu)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.MainMenuCommand:
                    CommandMainMenuCommand((SystemData.CommandData)viewEvent.Template);
                    break;
                case CommandType.SelectSideMenu:
                    CommandSelectSideMenu();
                    break;
            }
        }

        private void CommandMainMenuCommand(SystemData.CommandData commandData)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            switch (commandData.Key)
            {
                case "Departure":
                    _model.PartyInfo.DepartureCount.GainValue(1);
                    _view.CommandSceneChange(Scene.Dungeon);
                    break;
                case "DeckEdit":
                    CommandDeckEdit();
                    break;
                case "Mission":
                    CommandAchievement();
                    break;
                case "Status":
                    var actorInfos = _model.PartyInfo.ActorInfos;
                    CommandStatusInfo(actorInfos,false,true,true,false,actorInfos[0].ActorId.Value,() => 
                    {
                    },false,true);
                    break;
            }
        }

        private void CommandDeckEdit()
        {
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.DeckEdit,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        private void CommandAchievement()
        {
            // 達成状況更新
            _model.CheckAchievementConditions();

            // 達成報酬あれば遷移　なければリスト表示
            var getItemInfos = _model.AchievementGetItemInfos();
            if (getItemInfos.Count > 0)
            {
                var strategySceneInfo = new StrategySceneInfo
                {
                    ActorInfos = _model.PartyInfo.CurrentDeckActorInfos(),
                    InBattle = false
                };
                strategySceneInfo.GetItemInfos = getItemInfos;
                _view.CommandSceneChange(Scene.Strategy,strategySceneInfo);
                return;
            }
            ShowAchievementList();
        }

        private void ShowAchievementList()
        {
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Achievement,
                template = null,
                EndEvent = () =>
                {
                    _busy = false;
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
        }

        private void CommandSelectSideMenu()
        {
            _busy = true;
            CommandCallSideMenu(MakeListData(_model.SideMenu()), () =>
            {
                _busy = false;
            });
        }
    }
}