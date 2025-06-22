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
            _view.SetHelpInputInfo("CHARACTER_LIST");
            _view.SetCharacterList(MakeListData(_model.PartyInfo.ActorInfos,0));
            _view.OpenAnimation();
        }

        private void CommandEndOpenAnimation()
        {
            CheckTutorialState();
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
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
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Transfer)
            {
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
            if (actorInfo.ActorId.Value == _model.PartyInfo.ActorInfos[0].ActorId.Value)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                var cautionInfo = new CautionInfo();
                cautionInfo.SetTitle("ヒロインは派遣できません！");
                _view.CommandCallCaution(cautionInfo);
                return;
            }
            var confirmInfo = new ConfirmInfo(actorInfo.Master.Name + "を派遣しますか？\n！派遣した仲間は終末まで戻ってきません！", (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    _view.CallSystemCommand(Base.CommandType.ClosePopupAll);
                    _model.PartyInfo.TransferCommandCount.GainValue(1);
                    CheckAchievements();
                    _model.TransferGetItem(actorInfo);
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