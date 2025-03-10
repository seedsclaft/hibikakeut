using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class StatusAssign : MonoBehaviour
    {
        [SerializeField] private GameObject statusRoot = null;
        public GameObject StatusRoot => statusRoot;
        [SerializeField] private GameObject statusPrefab = null;
        [SerializeField] private GameObject enemyDetailPrefab = null;
        [SerializeField] private GameObject sideMenuPrefab = null;
        [SerializeField] private GameObject tacticsStatusPrefab = null;
        private BaseView _statusView;
        public GameObject CreatePopup(StatusType statusType,HelpWindow helpWindow)
        {
            var prefab = Instantiate(GetStatusObject(statusType));
            prefab.transform.SetParent(statusRoot.transform, false);
            statusRoot.SetActive(true);
            _statusView = prefab.GetComponent<BaseView>();
            _statusView?.SetHelpWindow(helpWindow);
            return prefab;
        }

        private GameObject GetStatusObject(StatusType statusType)
        {
            return statusType switch
            {
                StatusType.Status => statusPrefab,
                StatusType.EnemyDetail => enemyDetailPrefab,
                StatusType.SideMenu => sideMenuPrefab,
                StatusType.TacticsStatus => tacticsStatusPrefab,
                _ => null,
            };
        }    
        
        public void CloseStatus()
        {
            foreach(Transform child in statusRoot.transform)
            {
                Destroy(child.gameObject);
            }
            statusRoot.SetActive(false);
        }

        public void SetBusy(bool isBusy)
        {
            _statusView?.SetBusy(isBusy);
        }
    }

    public enum StatusType
    {
        Status,
        EnemyDetail,
        SideMenu,
        TacticsStatus,
    }
}