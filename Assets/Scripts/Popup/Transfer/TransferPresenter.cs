using System;
using System.Collections;
using System.Collections.Generic;
using Ryneus.Transfer;

namespace Ryneus
{
    public class TransferPresenter : BasePresenter
    {
        TransferModel _model = null;
        TransferView _view = null;

        private bool _busy = true;
        public TransferPresenter(TransferView view)
        {
            _view = view;

            SetView(_view);
            _view.SetEvent((type) => UpdateCommand(type));
            Initialize(true);
        }

        private void Initialize(bool first)
        {
            _model = new TransferModel();
            SetModel(_model);
             _view.OpenAnimation(first ? InitializeAfter : null);
            if (!first)
            {
                InitializeAfter();
            }
        }

        private void InitializeAfter()
        {
            _view.SetCharacterList(MakeListData(_model.PartyInfo.EditableActorInfos(), 0));
        }

        private void CommandEndOpenAnimation()
        {
            CheckTutorialState();
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Transfer)
            {
                return;
            }
            if (_busy || _view.AnimationBusy)
            {
                switch (viewEvent.ViewCommandType.CommandType)
                {
                    case CommandType.EndOpenAnimation:
                        CommandEndOpenAnimation();
                        break;
                }
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.Initialize:
                    Initialize(false);
                    break;
                case CommandType.DecideActor:
                    CommandDecideActor((ActorInfo)viewEvent.Template);
                    break;
            }
        }

        private void CommandDecideActor(ActorInfo actorInfo)
        {
            if (_model.PartyInfo.ActorInfos.Count == 0)
            {
                return;
            }
            if (!_model.EnableTransfer(actorInfo))
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                var textId = actorInfo.Master.Id > 100 ? 35020 : 35021;
                CommandCautionInfo(DataSystem.GetText(textId));
                return;
            }
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CallConfirmView(actorInfo.Master.Name + DataSystem.GetText(35010), (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    _view.CallSystemCommand(Base.CommandType.ClosePopupAll);
                    _model.PartyInfo.PartyStatInfo.TransferCommandCount.GainValue(1);
                    CheckAchievements();
                    var evaluate = _model.TransferGetItem(actorInfo);
                    var sceneParam = new MainMenuSceneInfo
                    {
                        CommandIndex = 5
                    };
                    var strategySceneInfo = new StrategySceneInfo
                    {
                        ActorInfos = new List<ActorInfo>(){actorInfo},
                        InBattle = false,
                        GetItemInfos = _model.TransferGetItemInfos(actorInfo),
                        ReturnMainMenuSceneParam = sceneParam
                    };
                    _view.CommandSceneChange(Scene.Strategy, strategySceneInfo);
                }
                _busy = false;
            });
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}