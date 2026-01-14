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
        public ViewCommandType(ViewCommandSceneType viewCommandSceneType, object template)
        {
            ViewCommandSceneType = viewCommandSceneType;
            CommandType = template;
        }
    }

    public class OtherViewEvent
    {
        public ViewCommandSceneType ViewCommandSceneType;
        public object CommandType;
        public object Templete;
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
        Strategy,
        Dungeon,
        Interlude,
        FileList,
        CharacterList,
        DeckEdit,
        Achievement,
        ItemList,
        ArtifactList,
        StageList,
        Transfer,
        Trade,
        UseItem,
        DungeonMap,
        LevelUp,
        SideMenu,
        Option,
        Confirm,
        Tutorial,
    }

    public interface IListViewItem
    {
        void UpdateViewItem();
        public T ListItemData<T>();
    }

    public interface IInputHandlerEvent
    {
        void InputHandler(List<InputKeyType> keyType, bool pressed);
        void MouseCancelHandler();
        void MouseMoveHandler(Vector3 position);
        void MouseWheelHandler(Vector2 position);
    }
}