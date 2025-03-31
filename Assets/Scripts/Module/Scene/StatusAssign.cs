using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class StatusAssign : MonoBehaviour
    {
        public GameObject StatusRoot => gameObject;
        private BaseView _statusView;
        public GameObject CreatePopup(StatusType statusType,HelpWindow helpWindow)
        {
            var prefab = Instantiate(GetStatusObject(statusType));
            prefab.transform.SetParent(transform, false);
            gameObject.SetActive(true);
            _statusView = prefab.GetComponent<BaseView>();
            _statusView?.SetHelpWindow(helpWindow);
            return prefab;
        }

        private GameObject GetStatusObject(StatusType statusType)
        {
            return ResourceSystem.LoadResource<GameObject>("Scenes/" + statusType + "Scene");
        }    
        
        public void CloseStatus()
        {
            foreach(Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            gameObject.SetActive(false);
        }

        public void SetBusy(bool isBusy)
        {
            _statusView?.SetBusy(isBusy);
        }
    }

    public enum StatusType
    {
        Status,
        EnemyInfo,
        TacticsStatus,
    }
}