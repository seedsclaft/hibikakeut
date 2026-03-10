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
            }
            UIComponent.SetActive(gameObject, true);
            prefab = _createdPopupPrefabs[popupType];
            _stackPopupPrefab.Add(prefab);
            UIComponent.SetActive(prefab, true);
            prefab.transform.SetAsLastSibling();
            return first;
        }

        private GameObject GetConfirmObject(ConfirmType popupType)
        {
            return ResourceSystem.LoadResource<GameObject>("Popups/Popup" + popupType);
        }

        public void CloseConfirm()
        {
            if (_stackPopupPrefab != null)
            {
                foreach (var stackPopupPrefab in _stackPopupPrefab)
                {
                    UIComponent.SetActive(stackPopupPrefab.gameObject, false);
                }
            }
            foreach(Transform child in transform)
            {
                //Destroy(child.gameObject);
            }
            UIComponent.SetActive(gameObject, false);
        }

        public void HideConfirm()
        {
            UIComponent.SetActive(gameObject, false);
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