using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class TacticsCharaLayer : MonoBehaviour
    {
        [SerializeField] private List<GameObject> tacticsCharaRoots;
        [SerializeField] private GameObject tacticsCharaPrefab;

        private List<TacticsChara> _tacticsCharacters = new ();

        public ActorInfo ActorInfo()
        {
            var clicked = _tacticsCharacters.Find(a => a.Cursor.activeSelf);
            clicked?.HideCursor();
            return clicked.ActorInfo;
        }

        public void SetData(List<ActorInfo> actorInfos,System.Action clickEvent)
        {
            _tacticsCharacters.ForEach(a => a.Hide());
            for (int i = 0; i < actorInfos.Count;i++)
            {
                if (tacticsCharaRoots.Count <= i)
                {
                    continue;
                }
                if (_tacticsCharacters.Count <= i)
                {
                    var prefab = Instantiate(tacticsCharaPrefab);
                    prefab.transform.SetParent(tacticsCharaRoots[i].transform, false);
                    var comp = prefab.GetComponent<TacticsChara>();
                    _tacticsCharacters.Add(comp);
                }
                var rectTransform = tacticsCharaRoots[i].GetComponent<RectTransform>();
                _tacticsCharacters[i].Show();
                _tacticsCharacters[i].Initialize(gameObject,rectTransform.localPosition.x,rectTransform.localPosition.y,rectTransform.localScale.x);
                _tacticsCharacters[i].SetData(actorInfos[i]);
                _tacticsCharacters[i].OnClickAddListener(clickEvent);
                _tacticsCharacters[i].SetIndex(actorInfos[i].ActorId.Value);
                _tacticsCharacters[i].SetSelectHandler((a) => 
                {
                    SelectedEvent(a);
                });
            }
        }

        private void SelectedEvent(int actorId)
        {
            foreach (var tacticsChara in _tacticsCharacters)
            {
                if (tacticsChara.ActorInfo != null)
                {
                    tacticsChara.SetSelectCursor(actorId == tacticsChara.ActorInfo.ActorId.Value);
                }
            }
        }

        public TacticsChara ZoomActor(int actorId)
        {
            var find = _tacticsCharacters.Find(a => a.ActorInfo?.ActorId.Value == actorId);
            if (find != null)
            {
                find.ZoomActor();
                HideWithOutActor(actorId);
                return find;
            }
            return null;
        }

        public void EndZoomActor()
        {
            foreach (var tacticsChara in _tacticsCharacters)
            {
                if (tacticsChara.ActorInfo != null)
                {
                    tacticsChara.EndZoomActor();
                }
            }
        }

        public void EndStatusCursor()
        {
            foreach (var tacticsChara in _tacticsCharacters)
            {
                if (tacticsChara.ActorInfo != null)
                {
                    tacticsChara.KillSequence();
                    tacticsChara.EndSelectCursor();
                }
            }
        } 
        
        public void ShowActor()
        {
            foreach (var tacticsChara in _tacticsCharacters)
            {
                if (tacticsChara.ActorInfo != null)
                {
                    tacticsChara.ShowActor();
                }
            }
        }

        public void HideWithOutActor(int withOutActorId)
        {
            foreach (var tacticsChara in _tacticsCharacters)
            {
                if (tacticsChara.ActorInfo != null && withOutActorId != tacticsChara.ActorInfo.ActorId.Value)
                {
                    tacticsChara.HideActor();
                }
            }
        }
    }
}