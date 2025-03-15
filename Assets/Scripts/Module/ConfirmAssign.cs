using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class ConfirmAssign : MonoBehaviour
    {
        [SerializeField] private GameObject confirmRoot = null;
        public GameObject CreateConfirm(ConfirmType popupType,HelpWindow helpWindow)
        {
            if (confirmRoot.transform.childCount > 0)
            {
                CloseConfirm();
            }
            var prefab = Instantiate(GetConfirmObject(popupType));
            prefab.transform.SetParent(confirmRoot.transform, false);
            confirmRoot.gameObject.SetActive(true);
            var view = prefab.GetComponent<BaseView>();
            view?.SetHelpWindow(helpWindow);
            return prefab;
        }

        private GameObject GetConfirmObject(ConfirmType popupType)
        {
            return ResourceSystem.LoadResource<GameObject>("Popups/Popup" + popupType);
        }

        public void CloseConfirm()
        {
            foreach(Transform child in confirmRoot.transform)
            {
                Destroy(child.gameObject);
            }
            confirmRoot.gameObject.SetActive(false);
        }

        public void HideConfirm()
        {
            confirmRoot.gameObject.SetActive(false);
        }
    }

    public enum ConfirmType
    {
        None,
        Confirm,
        Caution,
        SkillDetail,
    }
}