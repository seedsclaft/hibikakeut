using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class TraitsModel : BaseModel
    {
        private TraitsSceneInfo _sceneParam;
        public TraitsModel()
        {
            _sceneParam = (TraitsSceneInfo)GameSystem.SceneStackManager.LastPopupInfo.template;
        }

        public List<TraitsInfo> TraitsDates()
        {
            return _sceneParam.ActorInfo.TraitInfos;
        }
    }

    public class TraitsSceneInfo
    {
        public ActorInfo ActorInfo;
    }
}