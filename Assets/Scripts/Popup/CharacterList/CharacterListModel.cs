using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CharacterListModel : BaseModel
    {
        CharacterListInfo _sceneParam;
        private List<ActorInfo> _actorInfos = null;
        public List<ActorInfo> ActorInfos => _actorInfos;
        public CharacterListModel()
        {
            _sceneParam = (CharacterListInfo)GameSystem.SceneStackManager.LastTemplate;
            _actorInfos = _sceneParam.ActorInfos;
        }

        public void CallDecideEvent(ActorInfo actorInfo)
        {
            if (actorInfo == null)
            {
                return;
            }
            _sceneParam.CallEvent(actorInfo.ActorId.Value);
        }
    }    
    
    public class CharacterListInfo
    {
        private System.Action<int> _callEvent;
        public System.Action<int> CallEvent => _callEvent;
        public CharacterListInfo(System.Action<int> callEvent,System.Action backEvent)
        {
            _callEvent = callEvent;
            _backEvent = backEvent;
        }
        private System.Action _backEvent;
        public System.Action BackEvent => _backEvent;
        
        private List<ActorInfo> _actorInfos;
        public List<ActorInfo> ActorInfos => _actorInfos;
        public void SetActorInfos(List<ActorInfo> actorInfos)
        {
            _actorInfos = actorInfos;
        }
    }
}