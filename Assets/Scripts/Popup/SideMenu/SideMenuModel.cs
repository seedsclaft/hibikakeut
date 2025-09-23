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
    }
}
