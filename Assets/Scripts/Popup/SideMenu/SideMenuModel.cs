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
            // 全回復
            foreach (var actorInfo in PartyInfo.CurrentDeckActorInfos())
            {
                actorInfo.ChangeHp(actorInfo.MaxHp);
            }
            PartyInfo.Period.GainValue(1);
            SaveDungeonPlayerData();
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
