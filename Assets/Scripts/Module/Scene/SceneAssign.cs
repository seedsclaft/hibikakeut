using UnityEngine;

namespace Ryneus
{
    public class SceneAssign : MonoBehaviour
    {
        public GameObject CreateScene(Scene scene,HelpWindow helpWindow)
        {
            var prefab = Instantiate(GetSceneObject(scene));
            prefab.transform.SetParent(transform, false);
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