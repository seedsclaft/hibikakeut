using UnityEngine.EventSystems;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// The interface for notifications of event processor.
    /// </Summary>
    public interface IEventProcessor : IEventSystemHandler
    {
        /// <Summary>
        /// Send an event finished message.
        /// </Summary>
        void OnFinishedEvent();

        /// <Summary>
        /// Send a move finished message in the event of the opening door.
        /// </Summary>
        void OnPostMove();

        /// <Summary>
        /// Send a message to start the event of move position.
        /// </Summary>
        /// <param name="eventParts">The event parts data.</param>
        void OnMovePosition(DungeonMasterData destDungeon, FloorMapMasterData destFloor, Vector2Int destPos, DungeonDir destDirection, bool redrawDungeon, GameObject eventCallbackObj);

        /// <Summary>
        /// Send a exiting the dungeon message.
        /// </Summary>
        /// <param name="notifyObj">Send an exit notification from MoveController to this object.</param>
        void OnExitDungeon(GameObject notifyObj);
    }
}