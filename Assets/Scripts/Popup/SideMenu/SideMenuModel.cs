using System.Collections.Generic;

namespace Ryneus
{
    public class SideMenuModel : BaseModel
    {
        private SideMenuViewInfo _sceneParam;
        public SideMenuViewInfo SceneParam => _sceneParam;
        public SideMenuModel()
        {
            _sceneParam = (SideMenuViewInfo)GameSystem.SceneStackManager.LastTemplate;
        }
        public void DeletePlayerData()
        {
            SaveSystem.DeletePlayerData();
        }

        public void DeleteStageData()
        {
            SaveSystem.DeleteStageData();
        }

        public void ReturnDungeon()
        {
            if (CurrentDeckInfo == null)
            {
                return;
            }
            var playerDungeonId = Ariadne.PlayerPosition.currentDungeonId;
            var playerPosition = Ariadne.PlayerPosition.playerPos;
            var playerDirection = Ariadne.PlayerPosition.direction;
            CurrentDeckInfo.SetPosition(playerDungeonId,playerPosition.x,playerPosition.y,(int)playerDirection);
            // 開示マス情報を更新
            var traverses = Ariadne.TraverseManager.GetDungeonTraverseData(playerDungeonId);
            PartyInfo.AddDungeonTraverse(playerDungeonId,traverses.traverseDict);
        }

        public List<ActorInfo> CurrentDeckActorInfos()
        {
            var actorInfos = new List<ActorInfo>();
            if (CurrentDeckInfo != null)
            {
                actorInfos = PartyInfo.CurrentDeckActorInfos();
            }
            return actorInfos;
        }
    }
}
