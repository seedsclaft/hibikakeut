using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class SceneStackManager
    {
        private List<SceneInfo> _sceneInfo = new();
        public Scene LastScene => _sceneInfo.Count > 0 ? _sceneInfo[^1].FromScene : Scene.None;
        public Scene Current => _sceneInfo.Count > 0 ? _sceneInfo[^1].ToScene : Scene.None;
        public object LastSceneParam => _sceneInfo.Count > 0 ? _sceneInfo[^1].SceneParam : null;
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

        private List<PopupInfo> _popupInfo = new();
        public PopupInfo LastPopupInfo => _popupInfo.Count > 0 ? _popupInfo[^1] : null;
        public object LastTemplate => _popupInfo.Count > 0 ? _popupInfo[^1].template : null;
        
        public void PushPopupInfo(PopupInfo popupInfo)
        {
            if (_popupInfo.Contains(popupInfo))
            {
                return;
            }
            _popupInfo.Add(popupInfo);
        }

        public void RemovePopupInfo(PopupInfo popupInfo)
        {
            UnityEngine.Debug.LogError("RemovePopupInfo " + popupInfo.PopupType);
            _popupInfo.Remove(popupInfo);
        }

        public void ClearPopupInfo()
        {
            _popupInfo.Clear();
        }

        private List<StatusViewInfo> _statusViewInfo = new();
        public object LastStatusViewInfo => _statusViewInfo.Count > 0 ? _statusViewInfo[^1] : null;
        
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