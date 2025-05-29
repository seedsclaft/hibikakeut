using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CharacterListPresenter : BasePresenter
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
            Func<ActorInfo, bool> enable = (actorInfo) =>
            {
                // 既に出撃中か
                return !_model.NoDepatureActorIds().Contains(actorInfo.ActorId.Value);
            };
            _view.SetCharacterList(MakeListDataFunc<ActorInfo>(_model.PartyInfo.CurrentDeckActorInfos(), 0, enable));
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
                    case CharacterList.CommandType.EndOpenAnimation:
                        CommandEndOpenAnimation();
                        break;
                }
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.CharacterList)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CharacterList.CommandType.DecideActor:
                    CommandDecideActor((ActorInfo)viewEvent.Template);
                    break;
            }
        }

        private void CommandDecideActor(ActorInfo actorInfo)
        {
            // 既に出撃中であれば
            if (_model.NoDepatureActorIds().Contains(actorInfo.ActorId.Value))
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                CommandCautionInfo("出撃中の仲間は編成できません");
                return;
            }
            _view.BackEvent();
            _model.CallDecideEvent(actorInfo);
        }

        private void CheckTutorialState(object commandType = null)
        {
            Func<TutorialData, bool> enable = (tutorialData) =>
            {
                var checkFlag = false;
                if (tutorialData.Param1 == 200)
                {
                    // 出撃選択
                    checkFlag = true;
                }
                return checkFlag;
            };
            Func<TutorialData, bool> checkEnd = (tutorialData) =>
            {
                return true;
            };
            var tutorialViewInfo = new TutorialViewInfo
            {
                SceneType = (int)PopupType.CharacterList + 100,
                CheckEndMethod = checkEnd,
                CheckMethod = enable,
                EndEvent = () =>
                {
                    _busy = false;
                    CheckTutorialState(commandType);
                }
            };
            _view.CommandCheckTutorialState(tutorialViewInfo);
        }
    }
}