using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.UI;


namespace Ryneus
{
    public class MultiScroller : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler 
    {
        [SerializeField]
        private IDragHandler[] parentDragHandlers;
        private IBeginDragHandler[] parentBeginDragHandlers;
        private IEndDragHandler[] parentEndDragHandlers;

        private bool isSelf = false;
        public void SetScrollEvent(ScrollRect scrollRect) 
        {
            parentDragHandlers = scrollRect.gameObject.GetComponents<IDragHandler>().Where(p => p is not MultiScroller).ToArray();
            parentBeginDragHandlers = scrollRect.gameObject.GetComponents<IBeginDragHandler>().Where(p => p is not MultiScroller).ToArray();
            parentEndDragHandlers = scrollRect.gameObject.GetComponents<IEndDragHandler>().Where(p => p is not MultiScroller).ToArray();
        }
        
        public void OnDrag(PointerEventData ped)
        {
            if (parentDragHandlers == null)
            {
                return;
            }
            foreach(var dr in parentDragHandlers)
            {
                dr.OnDrag(ped);
            }
        }

        public void OnBeginDrag(PointerEventData ped)
        {
            if (parentBeginDragHandlers == null)
            {
                return;
            }
            foreach(var dr in parentBeginDragHandlers)
            {
                dr.OnBeginDrag(ped);
            }
        }

        public void OnEndDrag(PointerEventData ped)
        {
            if (parentEndDragHandlers == null)
            {
                return;
            }
            foreach(var dr in parentEndDragHandlers)
            {
                dr.OnEndDrag(ped);
            }
        }
    }
}