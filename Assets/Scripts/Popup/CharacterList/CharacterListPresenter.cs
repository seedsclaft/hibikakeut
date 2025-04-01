using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CharacterListPresenter :BasePresenter
    {
        CharacterListModel _model = null;
        CharacterListView _view = null;

        private bool _busy = true;
        public CharacterListPresenter(CharacterListView view)
        {
            _view = view;
            _model = new CharacterListModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetHelpInputInfo("CHARACTER_LIST");
            _view.SetCharacterList(MakeListData(_model.ActorInfos));
            _view.OpenAnimation();
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.CharacterList)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CharacterList.CommandType.DecideActor:
                    CommandDecideActor((ActorInfo)viewEvent.template);
                    break;
            }
        }

        private void CommandDecideActor(ActorInfo actorInfo)
        {
            _view.BackEvent();
            _model.CallDecideEvent(actorInfo);
        }
    }
}