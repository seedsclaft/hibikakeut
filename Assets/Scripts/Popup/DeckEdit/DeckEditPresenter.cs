using System.Collections;
using System.Collections.Generic;
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

            SetView(_view);
            _view.SetEvent((type) => UpdateCommand(type));
            Initialize(true);
            _busy = false;
        }

        private void Initialize(bool first)
        {
            _model = new DeckEditModel();
            SetModel(_model);
            _view.OpenAnimation(first ? InitializeAfter : null);
            if (!first)
            {
                InitializeAfter();
            }
        }

        private void InitializeAfter()
        {
            _view.SetPartyUnitList(MakeListData(_model.PartyUnit(), -1));
            _view.SetActorList(MakeListData(_model.PartyInfo.EditableActorInfos(), 0));
            _view.SelectChangeBattler(0);
            _view.UpdateActorInfo(_model.PartyInfo.EditableActorInfos().Find(a => _model.PartyUnit()[0].ActorInfo != null && a.ActorId.Value == _model.PartyUnit()[0].ActorInfo.ActorId.Value));
            _view.EndSelectChangeBattler();
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
                case CommandType.Initialize:
                    Initialize(false);
                    break;
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
                case CommandType.AutoDeck:
                    CommandAutoDeck();
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
            _view.CandidateSelectIndex(_model.FromEditSelectBattlerIndex());
        }

        private void CommandDecideBattlerInfo(ActorInfo actorInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.PartyInfo.PartyStatInfo.DeckEditCommandCount.GainValue(1);
            CheckAchievements();
            _model.SwapBattler(actorInfo.ActorId.Value);
            _view.EndSelectChangeBattler();
            CommandRefresh();
            _view.UpdateActorInfo(actorInfo);
        }

        private void CommandAutoDeck()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.PartyInfo.PartyStatInfo.DeckEditCommandCount.GainValue(1);
            CheckAchievements();
            _model.AutoDeck();
            _view.EndSelectChangeBattler();
            CommandRefresh();
            _view.UpdateActorInfo(_model.PartyInfo.ActorInfos[0]);
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
            // 整列が必要であれば整列する
            if (_model.AdjustEditIndexes())
            {
                CommandCautionInfo(DataSystem.GetText(43010));
            }
            _view.EndPopup();
        }

        private void CommandRefresh()
        {
            _view.SetPartyUnitList(MakeListData(_model.PartyUnit()));
            _view.SetActorList(MakeListData(_model.PartyInfo.ActorInfos));
        }
    }
}