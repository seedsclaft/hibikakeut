using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class SceneStackManager
    {
        private List<SceneInfo> _sceneInfo = new ();
        public Scene LastScene => _sceneInfo.Count > 0 ? _sceneInfo[_sceneInfo.Count-1].FromScene : Scene.None;
        public Scene Current => _sceneInfo.Count > 0 ? _sceneInfo[_sceneInfo.Count-1].ToScene : Scene.None;
        public object LastSceneParam => _sceneInfo.Count > 0 ? _sceneInfo[_sceneInfo.Count-1].SceneParam : null;
        public void PushSceneInfo(SceneInfo sceneInfo)
        {
            if (sceneInfo.SceneChangeType == SceneChangeType.Goto)
            {
                _sceneInfo.Clear();
                _sceneInfo.Add(sceneInfo);
            }
            if (sceneInfo.SceneChangeType == SceneChangeType.Push)
            {
                _sceneInfo.Add(sceneInfo);
            }
            if (LastScene != Scene.None && sceneInfo.SceneChangeType == SceneChangeType.Pop)
            {
                _sceneInfo.RemoveAt(_sceneInfo.Count-1);
                _sceneInfo.Add(sceneInfo);
            }
        }
        
        private List<PopupInfo> _popupInfo = new ();
        public object LastTemplate => _popupInfo.Count > 0 ? _popupInfo[_popupInfo.Count-1].template : null;
        
        public void PushPopupInfo(PopupInfo popupInfo)
        {
            _popupInfo.Clear();
            _popupInfo.Add(popupInfo);
        }

        private List<StatusViewInfo> _statusViewInfo = new ();
        public object LastStatusViewInfo => _statusViewInfo.Count > 0 ? _statusViewInfo[_statusViewInfo.Count-1] : null;
        
        public void PushStatusViewInfo(StatusViewInfo statusViewInfo)
        {
            _statusViewInfo.Clear();
            _statusViewInfo.Add(statusViewInfo);
        }
    }

    public class SceneInfo
    {
        public Scene FromScene;
        public Scene ToScene;
        public SceneChangeType SceneChangeType;
        public object SceneParam;
    }

    public enum SceneChangeType
    {
        None = 0,
        Push = 1,
        Pop = 2,
        Goto = 3
    }
}