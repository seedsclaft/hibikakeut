using System.Collections.Generic;
using UnityEngine;
using UtageExtensions;

namespace Ryneus
{
    public class MapAssign : MonoBehaviour
    {
        private string _lastMapName = "";
        public string LastMapName => _lastMapName;

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

        public void SetLastMapName(string mapName)
        {
            if (_lastMapName != mapName)
            {
                _lastMapName = mapName;
            }
        }
    }
}
