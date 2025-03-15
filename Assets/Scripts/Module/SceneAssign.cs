using UnityEngine;

namespace Ryneus
{
    public class SceneAssign : MonoBehaviour
    {
        [SerializeField] private GameObject uiRoot = null;
        public GameObject CreateScene(Scene scene,HelpWindow helpWindow)
        {
            var prefab = Instantiate(GetSceneObject(scene));
            prefab.transform.SetParent(uiRoot.transform, false);
            var view = prefab.GetComponent<BaseView>();
            view?.SetHelpWindow(helpWindow);
            return prefab;
        }

        private GameObject GetSceneObject(Scene scene)
        {
            return ResourceSystem.LoadResource<GameObject>("Scenes/" + scene + "Scene");
        }
    }
}