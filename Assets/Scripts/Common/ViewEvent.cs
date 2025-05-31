using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class ViewEvent
    {
        public object Template;
        public ViewCommandType ViewCommandType;

        public ViewEvent(ViewCommandType viewCommandType)
        {
            ViewCommandType = viewCommandType;
        }
    }

    public class ViewCommandType
    {
        public ViewCommandSceneType ViewCommandSceneType;
        public object CommandType;
        public ViewCommandType(ViewCommandSceneType viewCommandSceneType,object template)
        {
            ViewCommandSceneType = viewCommandSceneType;
            CommandType = template;
        }
    }

    public enum ViewCommandSceneType
    {
        None,
        System,
        Boot,
        Title,
        NameEntry,
        MainMenu,
        Tactics,
        Status,
        Battle,
        BattleParty,
        Strategy,
        Dungeon,
        FileList,
        CharacterList,
        UnitInfoList,
        DeckEdit,
        Achievement,
        ItemList,
        StageList,
        SideMenu,
        Option,
        TutorialStage,
        Confirm,
    }

    public interface IListViewItem
    {
        void UpdateViewItem();
        public T ListItemData<T>();
    }

    public interface IInputHandlerEvent
    {
        void InputHandler(List<InputKeyType> keyType,bool pressed);
        void MouseCancelHandler();
        void MouseMoveHandler(Vector3 position);
        void MouseWheelHandler(Vector2 position);
    }
}