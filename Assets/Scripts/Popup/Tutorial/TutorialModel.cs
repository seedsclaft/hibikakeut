using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class TutorialModel : BaseModel
    {
        private TutorialData _tutorialData;
        public TutorialData TutorialData => _tutorialData;
        public TutorialModel()
        {
            TutorialSceneInfo SceneParam = (TutorialSceneInfo)GameSystem.SceneStackManager.LastTemplate;
            _tutorialData = SceneParam.TutorialData;
        }
    }

    public class TutorialSceneInfo
    {
        public TutorialData TutorialData;
    }
}