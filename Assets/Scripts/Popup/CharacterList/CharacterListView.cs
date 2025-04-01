using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class CharacterListView : BaseView
    {
        [SerializeField] private BaseList characterList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        
        public override void Initialize() 
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.CharacterList);
            InitializeCharacterList();
            SetBaseAnimation(popupAnimation);
            new CharacterListPresenter(this);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform,null);
        }

        private void InitializeCharacterList()
        {
            characterList.Initialize();
            characterList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            characterList.SetInputHandler(InputKeyType.Decide,() => CallViewEvent(CharacterList.CommandType.DecideActor,characterList.ListItemData<ActorInfo>()));
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
            None = 0,
            DecideActor = 1,
        }
    }
}
