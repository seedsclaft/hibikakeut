using System;
using System.Collections;
using System.Collections.Generic;

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
            _model = new TransferModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetCharacterList(MakeListData(_model.PartyInfo.EditableActorInfos(), 0));
            _view.OpenAnimation();
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
                    case Transfer.CommandType.EndOpenAnimation:
                        CommandEndOpenAnimation();
                        break;
                }
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case Transfer.CommandType.DecideActor:
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
                var cautionInfo = new CautionInfo();
                var textId = actorInfo.Master.Id > 100 ? 35020 : 35021;
                cautionInfo.SetTitle(DataSystem.GetText(textId));
                _view.CommandCallCaution(cautionInfo);
                return;
            }
            _busy = true;
            _view.SetBusy(true);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var confirmInfo = new ConfirmInfo(actorInfo.Master.Name + DataSystem.GetText(35010), (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    _view.CallSystemCommand(Base.CommandType.ClosePopupAll);
                    _model.PartyInfo.TransferCommandCount.GainValue(1);
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
                _view.SetBusy(false);
            });
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}