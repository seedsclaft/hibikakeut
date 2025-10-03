using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ryneus.Transfer;

namespace Ryneus
{
    public class TransferView : BaseView
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
            SetViewCommandSceneType(ViewCommandSceneType.Transfer);
            InitializeTransfer();
            SetBaseAnimation(popupAnimation);
            _ = new TransferPresenter(this);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform, () => CallViewEvent(Transfer.CommandType.EndOpenAnimation));
        }

        private void InitializeTransfer()
        {
            characterList.Initialize();
            characterList.SetInputHandler(InputKeyType.Cancel, () => BackEvent());
            characterList.SetInputHandler(InputKeyType.Decide, () => CallViewEvent(Transfer.CommandType.DecideActor, characterList.ListItemData<ActorInfo>()));
            AddViewActives(characterList);
        }

        public void SetCharacterList(List<ListData> characterLists)
        {
            characterList.SetData(characterLists);
            characterList.Activate();
        }
    }

    namespace Transfer
    {
        public enum CommandType
        {
            Initialize = 0,
            DecideActor = 1,
            EndOpenAnimation = 2,
        }
    }
}
