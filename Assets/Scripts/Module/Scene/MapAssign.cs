using System.Collections.Generic;
using UnityEngine;
using UtageExtensions;

namespace Ryneus
{
    public class MapAssign : MonoBehaviour
    {
        public GameObject CreateMap(string mapName)
        {
            var prefab = Instantiate(GetMapObject(mapName));
            prefab.transform.SetParent(transform, false);
            return prefab;
        }

        public void CreateMapObject(GameObject gameObject)
        {
            gameObject.transform.SetParent(transform, false);
        }

        private GameObject GetMapObject(string scene)
        {
            return ResourceSystem.LoadResource<GameObject>("Dungeons/" + scene);
        }

        public void ClearMap()
        {
            transform.DestroyChildren();
        }
    }

    public enum MapType
    {
        Default = 0,
        Battle,
    }
}
