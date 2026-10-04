using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ryneus.Transfer;
using System;

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

        public void OpenAnimation(Action initializeAfter)
        {
            popupAnimation.OpenAnimation(UiRoot.transform, () => 
            {
                initializeAfter?.Invoke();
                CallViewEvent(CommandType.EndOpenAnimation);
            });
        }

        private void InitializeTransfer()
        {
            characterList.Initialize();
            characterList.SetInputHandler(InputKeyType.Cancel, () => BackEvent());
            characterList.SetInputHandler(InputKeyType.Decide, () => CallViewEvent(CommandType.DecideActor, characterList.ListItemData<ActorInfo>()));
            characterList.SetInputHandler(InputKeyType.Option1, () => CallViewEvent(CommandType.DetailActor, characterList.ListItemData<ActorInfo>()));
            AddViewActives(characterList);
        }

        public void SetCharacterList(List<ListData> characterLists)
        {
            characterList.SetData(characterLists, true, () =>
            {
                foreach (var prefab in characterList.ItemPrefabList)
                {
                    var comp = prefab.GetComponent<CharacterListActor>();
                    if (comp != null)
                    {
                        comp.SetDetailEvent((a) =>
                        {
                            CallViewEvent(CommandType.DetailActor, a);
                        });
                    }
                }
            });
            characterList.Activate();
        }
    }

    namespace Transfer
    {
        public enum CommandType
        {
            Initialize = 0,
            DecideActor,
            DetailActor,
            EndOpenAnimation,
        }
    }
}
