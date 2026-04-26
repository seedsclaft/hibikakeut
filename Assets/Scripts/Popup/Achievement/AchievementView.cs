using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ryneus.Achievement;
using System;

namespace Ryneus
{
    public class AchievementView : BaseView
    {
        [SerializeField] private BaseList achievementList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        [SerializeField] private PartyInfoComponent partyInfoComponent;


        public override void Initialize()
        {
            if (IsInitilized)
            {
                CallViewEvent(CommandType.Initialize);
                return;
            }
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Achievement);
            InitializeAchievement();
            SetBaseAnimation(popupAnimation);
            _ = new AchievementPresenter(this);
        }

        public void OpenAnimation(Action initializeAfter)
        {
            popupAnimation.OpenAnimation(UiRoot.transform, initializeAfter);
        }

        private void InitializeAchievement()
        {
            achievementList.Initialize();
            achievementList.SetInputHandler(InputKeyType.Cancel, () => BackEvent());
            //characterList.SetInputHandler(InputKeyType.Decide, () => CallViewEvent(Achievement.CommandType.DecideActor, characterList.ListItemData<ActorInfo>()));
            SetInputHandler(achievementList.gameObject);
        }

        public void SetAchievement(List<ListData> achievementLists)
        {
            achievementList.SetData(achievementLists);
            achievementList.Activate();
            partyInfoComponent.UpdateCurrentInfo();
        }
    }

    namespace Achievement
    {
        public enum CommandType
        {
            Initialize = 0,
        }
    }
}
