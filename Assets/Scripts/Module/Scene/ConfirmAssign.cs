using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class ConfirmAssign : MonoBehaviour
    {
        public GameObject CreateConfirm(ConfirmType popupType,HelpWindow helpWindow)
        {
            if (transform.childCount > 0)
            {
                CloseConfirm();
            }
            var prefab = Instantiate(GetConfirmObject(popupType));
            prefab.transform.SetParent(transform, false);
            gameObject.SetActive(true);
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
    }
}