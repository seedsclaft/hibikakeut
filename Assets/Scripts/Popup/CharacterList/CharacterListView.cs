using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ryneus.CharacterList;

namespace Ryneus
{
    public class CharacterListView : BaseView
    {
        [SerializeField] private BaseList characterList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;


        public override void Initialize()
        {
            if (IsInitilized)
            {
                CallViewEvent(CommandType.Initialize);
                return;
            }
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.CharacterList);
            InitializeCharacterList();
            SetBaseAnimation(popupAnimation);
            _ = new CharacterListPresenter(this);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform, () => CallViewEvent(CharacterList.CommandType.EndOpenAnimation));
        }

        private void InitializeCharacterList()
        {
            characterList.Initialize();
            characterList.SetInputHandler(InputKeyType.Cancel, () => BackEvent());
            characterList.SetInputHandler(InputKeyType.Decide, () => CallViewEvent(CharacterList.CommandType.DecideActor, characterList.ListItemData<ActorInfo>()));
            SetInputHandler(characterList.gameObject);
        }

        public void SetCharacterList(List<ListData> characterLists)
        {
            characterList.SetData(characterLists);
            characterList.Activate();
        }
    }

    namespace CharacterList
    {
        public enum CommandType
        {
            Initialize,
            DecideActor = 1,
            EndOpenAnimation = 2,
        }
    }
}
