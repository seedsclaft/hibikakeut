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
            CommandRefresh();
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
                    CommandSelectBattler((BattlerInfo)viewEvent.Template);
                    break;
                case CommandType.DecideBattlerInfo:
                    CommandDecideBattlerInfo((ActorInfo)viewEvent.Template);
                    break;
                case CommandType.SelectingActorInfo:
                case CommandType.SelectingBattlerInfo:
                    _view.UpdateActorInfo((ActorInfo)viewEvent.Template);
                    break;
                    
            }
        }

        private void CommandSelectBattler(BattlerInfo battlerInfo)
        {
            _model.ChangeActorId.SetValue(battlerInfo.ActorInfo.ActorId.Value);
            _view.SelectChangeBattler();
        }

        private void CommandDecideBattlerInfo(ActorInfo actorInfo)
        {
            _model.SwapBattler(actorInfo.ActorId.Value);
            CommandRefresh();
            _view.EndSelectChangeBattler();
        }

        private void CommandRefresh()
        {
            _view.SetPartyUnitList(MakeListData(_model.PartyUnit(),-1));
            _view.SetActorList(MakeListData(_model.PartyInfo.ActorInfos,-1));
        }
    }
}