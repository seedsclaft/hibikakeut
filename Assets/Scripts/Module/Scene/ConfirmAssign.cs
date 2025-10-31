using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class ConfirmAssign : MonoBehaviour
    {
        private Dictionary<ConfirmType, GameObject> _createdPopupPrefabs = new();
        public List<GameObject> _stackPopupPrefab = new();
        public GameObject LastPopupPrefab => _stackPopupPrefab.Count > 0 ? _stackPopupPrefab[_stackPopupPrefab.Count - 1] : null;

        public bool CreateConfirm(ConfirmType popupType, HelpWindow helpWindow)
        {
            bool first = false;
            GameObject prefab;
            if (!_createdPopupPrefabs.ContainsKey(popupType))
            {
                prefab = Instantiate(GetConfirmObject(popupType));
                prefab.transform.SetParent(transform, false);
                _createdPopupPrefabs[popupType] = prefab;
                first = true;
                //CloseConfirm();
            }
            gameObject.SetActive(true);
            prefab = _createdPopupPrefabs[popupType];
            _stackPopupPrefab.Add(prefab);
            prefab.SetActive(true);
            prefab.transform.SetAsLastSibling();
            return first;
        }

        private GameObject GetConfirmObject(ConfirmType popupType)
        {
            return ResourceSystem.LoadResource<GameObject>("Popups/Popup" + popupType);
        }

        public void CloseConfirm()
        {
            foreach(Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            gameObject.SetActive(false);
        }

        public void HideConfirm()
        {
            gameObject.SetActive(false);
        }
    }

    public enum ConfirmType
    {
        None,
        Confirm,
        Caution,
        SkillDetail,
        MissionClear,
        StageConfirm,
        NewStageAlert,
        ItemDetail,
    }
}