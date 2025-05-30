using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ariadne
{
    /// <Summary>
    /// Manager class of drawing dungeon.
    /// This class notifies drawing message to dungeon object's parents.
    /// </Summary>
    public class DrawManager : MonoBehaviour, IDungeonObjects
    {
        protected GameObject groundObj;
        protected GameObject wallObj;
        protected GameObject ceilingObj;
        protected GameObject mapObj;
        protected List<GameObject> objList;

        [SerializeField]
        protected bool enableDrawMap = false;

        protected virtual void Start()
        {
            SetObjRef();
            SetParentList();
        }

        /// <Summary>
        /// Set object references to cache them.
        /// </Summary>
        protected virtual void SetObjRef()
        {
            if (groundObj == null)
            {
                groundObj = GameObject.Find(AriadneSceneObjectName.GroundParent);
            }

            if (wallObj == null)
            {
                wallObj = GameObject.Find(AriadneSceneObjectName.WallParent);
            }
            
            if (ceilingObj == null)
            {
                ceilingObj = GameObject.Find(AriadneSceneObjectName.CeilingParent);
            }
            
            if (mapObj == null)
            {
                mapObj = GameObject.Find(AriadneSceneObjectName.MapParent);
            }
        }

        /// <Summary>
        /// Set object list of dungeon object's parents.
        /// </Summary>
        protected virtual void SetParentList()
        {
            objList = new List<GameObject>();
            objList.Add(groundObj);
            objList.Add(wallObj);
            objList.Add(ceilingObj);
            objList.Add(mapObj);
        }

        /// <Summary>
        /// Send draw message to each parent object.
        /// </Summary>
        public virtual void OnDrawObj()
        {
            SetObjRef();
            SetParentList();

            foreach (GameObject obj in objList)
            {
                SendDrawMsg(obj);
            }
        }

        /// <Summary>
        /// Send redraw message to each parent object.
        /// </Summary>
        public virtual void OnRedrawObj()
        {
            SetObjRef();
            SetParentList();

            foreach (GameObject obj in objList)
            {
                SendRedrawMsg(obj);
            }
        }

        /// <Summary>
        /// Send remove message to each parent object.
        /// </Summary>
        public virtual void OnRemoveObj()
        {
            SetObjRef();
            SetParentList();
            
            foreach (GameObject obj in objList)
            {
                SendRemoveMsg(obj);
            }
        }
        
        protected virtual void Update()
        {
            if (mapObj == null)
            {
                return;
            }
            if (enableDrawMap)
            {
                mapObj.SetActive(true);
            }
            else
            {
                mapObj.SetActive(false);
            }
        }

        /// <Summary>
        /// Send messages for drawing dungeon objects.
        /// </Summary>
        /// <param name="obj">The parent object of dungeon parts.</param>
        protected virtual void SendDrawMsg(GameObject obj)
        {
            ExecuteEvents.Execute<IDrawer>(
                target: obj,
                eventData: null,
                functor: CallDraw
            );
        }

        /// <Summary>
        /// The functor of SendDrawMsg method.
        /// </Summary>
        void CallDraw(IDrawer drawer, BaseEventData eventData)
        {
            drawer.OnDraw();
        }

        /// <Summary>
        /// Send messages for re-drawing dungeon objects.
        /// </Summary>
        /// <param name="obj">The parent object of dungeon parts.</param>
        protected virtual void SendRedrawMsg(GameObject obj)
        {
            ExecuteEvents.Execute<IDrawer>(
                target: obj,
                eventData: null,
                functor: CallRedraw
            );
        }

        /// <Summary>
        /// The functor of SendRedrawMsg method.
        /// </Summary>
        void CallRedraw(IDrawer drawer, BaseEventData eventData)
        {
            drawer.OnRedraw();
        }

        /// <Summary>
        /// Send messages for removing dungeon objects.
        /// </Summary>
        /// <param name="obj">The parent object of dungeon parts.</param>
        protected virtual void SendRemoveMsg(GameObject obj)
        {
            ExecuteEvents.Execute<IDrawer>(
                target: obj,
                eventData: null,
                functor: CallRemoveObjects
            );
        }

        /// <Summary>
        /// The functor of SendRemoveMsg method.
        /// </Summary>
        void CallRemoveObjects(IDrawer drawer, BaseEventData eventData)
        {
            drawer.OnRemoveObjects();
        }
    }
}