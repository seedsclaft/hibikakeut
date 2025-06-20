using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ryneus.DeckEdit;

namespace Ryneus
{
    public class DeckEditPresenter : BasePresenter
    {
        DeckEditModel _model = null;
        DeckEditView _view = null;

        private bool _busy = true;
        public DeckEditPresenter(DeckEditView view)
        {
            _view = view;
            _model = new DeckEditModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
            _busy = false;
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetPartyUnitList(MakeListData(_model.PartyUnit(),-1));
            _view.SetActorList(MakeListData(_model.PartyInfo.ActorInfos,-1));
            _view.SelectChangeBattler(-1);
            _view.UpdateActorInfo(_model.PartyInfo.ActorInfos.Find(a => _model.PartyUnit()[0].ActorInfo != null && a.ActorId.Value == _model.PartyUnit()[0].ActorInfo.ActorId.Value));
            _view.EndSelectChangeBattler();
            _view.OpenAnimation();
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.DeckEdit)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.SelectBattler:
                    CommandSelectBattler((int)viewEvent.Template);
                    break;
                case CommandType.DecideBattlerInfo:
                    CommandDecideBattlerInfo((ActorInfo)viewEvent.Template);
                    break;
                case CommandType.SelectingActorInfo:
                case CommandType.SelectingBattlerInfo:
                    _view.UpdateActorInfo((ActorInfo)viewEvent.Template);
                    break;
                case CommandType.Back:
                    CommandBack();
                    break;
            }
        }

        private void CommandSelectBattler(int fromEditIndex)
        {
            if (fromEditIndex < 0)
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.FromEditIndex.SetValue(fromEditIndex + 1);
            _view.SelectChangeBattler(_model.FromEditSelectIndex());
        }

        private void CommandDecideBattlerInfo(ActorInfo actorInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.PartyInfo.DeckEditCommandCount.GainValue(1);
            CheckAchievements();
            _model.SwapBattler(actorInfo.ActorId.Value);
            _view.EndSelectChangeBattler();
            CommandRefresh();
            _view.UpdateActorInfo(actorInfo);
        }

        private void CommandBack()
        {
            if (_model.FromEditIndex.Value >= 0)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                _model.FromEditIndex.SetValue(-1);
                _view.EndSelectChangeBattler();
                CommandRefresh();
                return;
            }
            _view.BackEvent();
        }

        private void CommandRefresh()
        {
            _view.SetPartyUnitList(MakeListData(_model.PartyUnit()));
            _view.SetActorList(MakeListData(_model.PartyInfo.ActorInfos));
        }
    }
}