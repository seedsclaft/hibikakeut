using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class PopupAssign : MonoBehaviour
    {
        [SerializeField] private GameObject confirmRoot = null;

        private List<BaseView> _stackPopupView = new ();
        public List<BaseView> StackPopupView => _stackPopupView;
        public BaseView LastPopupView => _stackPopupView.Count > 0 ? _stackPopupView[_stackPopupView.Count-1] : null;
        
        public GameObject CreatePopup(PopupType popupType,HelpWindow helpWindow)
        {
            var prefab = Instantiate(GetPopupObject(popupType));
            prefab.transform.SetParent(confirmRoot.transform, false);
            confirmRoot.SetActive(true);
            var view = prefab.GetComponent<BaseView>();
            view?.SetHelpWindow(helpWindow);
            _stackPopupView.Add(view);
            return prefab;
        }

        private GameObject GetPopupObject(PopupType popupType)
        {
            return ResourceSystem.LoadResource<GameObject>("Popups/Popup" + popupType);
        }

        public void ClosePopup()
        {
            if (_stackPopupView.Count > 0)
            {
                var lastPopupView = _stackPopupView[_stackPopupView.Count-1];
                _stackPopupView.Remove(lastPopupView);
                Destroy(lastPopupView.gameObject);
            }
            if (_stackPopupView.Count == 0)
            {
                confirmRoot.SetActive(false);
            }
        }

        public void ClosePopupAll()
        {
            if (_stackPopupView.Count > 0)
            {
                confirmRoot.transform.DetachChildren();
                _stackPopupView.Clear();
            }
            confirmRoot.SetActive(false);
        }

        public void CloseTutorialPopup()
        {
            if (_stackPopupView.Count > 0)
            {
                var findIndex = _stackPopupView.FindIndex(a => a.GetType() == typeof(TutorialView));
                if (findIndex != -1)
                {
                    var lastPopupView = _stackPopupView[findIndex];
                    _stackPopupView.Remove(lastPopupView);
                    Destroy(lastPopupView.gameObject);
                }
            }
            if (_stackPopupView.Count == 0)
            {
                confirmRoot.SetActive(false);
            }
        }
    }

    public enum PopupType
    {
        None,
        SkillDetail,
        Ruling,
        Option,
        Ranking,
        Credit,
        CharacterList,
        Help,
        AlcanaList,
        SlotSave,
        LearnSkill,
        SkillTrigger,
        SkillLog,
        ScorePrize,
        ClearParty,
        CheckConflict,
        Guide,
        BattleParty,
        SideMenu,
        Dictionary,
        FileList,
        Tutorial,
    }
}