using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class PopupAssign : MonoBehaviour
    {
        private List<BaseView> _stackPopupView = new();
        public List<BaseView> StackPopupView => _stackPopupView;
        public BaseView LastPopupView => _stackPopupView.Count > 0 ? _stackPopupView[_stackPopupView.Count - 1] : null;

        public List<GameObject> _stackPopupPrefab = new();
        public GameObject LastPopupPrefab => _stackPopupPrefab.Count > 0 ? _stackPopupPrefab[_stackPopupPrefab.Count - 1] : null;

        private Dictionary<PopupType, GameObject> _createdPopupPrefabs = new();

        public void Initilize()
        {
            foreach (Transform child in gameObject.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public bool CreatePopup(PopupType popupType, HelpWindow helpWindow)
        {
            bool first = false;
            BaseView view;
            GameObject prefab;
            if (!_createdPopupPrefabs.ContainsKey(popupType))
            {
                prefab = Instantiate(GetPopupObject(popupType));
                prefab.transform.SetParent(transform, false);
                //view?.SetHelpWindow(helpWindow);
                _createdPopupPrefabs[popupType] = prefab;
                first = true;
                //view = prefab.GetComponent<BaseView>();
                //view.Initialize();
            }
            gameObject.SetActive(true);
            prefab = _createdPopupPrefabs[popupType];
            view = prefab.GetComponent<BaseView>();
            _stackPopupView.Add(view);
            _stackPopupPrefab.Add(prefab);
            prefab.SetActive(true);
            prefab.transform.SetAsLastSibling();
            return first;
        }

        private GameObject GetPopupObject(PopupType popupType)
        {
            return ResourceSystem.LoadResource<GameObject>("Popups/Popup" + popupType);
        }

        public void ClosePopup()
        {
            if (_stackPopupView.Count > 0)
            {
                LastPopupView.Dispose();
                LastPopupPrefab.SetActive(false);
                _stackPopupView.Remove(LastPopupView);
                _stackPopupPrefab.Remove(LastPopupPrefab);
                //Destroy(lastPopupView.gameObject);
            }
            if (_stackPopupView.Count == 0)
            {
                foreach (var createdPopupPrefab in _createdPopupPrefabs)
                {
                    createdPopupPrefab.Value.SetActive(false);
                }
                gameObject.SetActive(false);
            }
        }

        public void ClosePopupAll()
        {
            if (_stackPopupView.Count > 0)
            {
                foreach (var createdPopupPrefab in _createdPopupPrefabs)
                {
                    createdPopupPrefab.Value.SetActive(false);
                }
            }
            foreach (var stackPopupView in _stackPopupView)
            {
                stackPopupView.Dispose();
            }
            _stackPopupView.Clear();
            gameObject.SetActive(false);
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
                gameObject.SetActive(false);
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
        UnitInfoList,
        DepatureList,
        DeckEdit,
        Help,
        Achievement,
        ItemList,
        ArtifactList,
        StageList,
        Transfer,
        ReleaseList,
        Trade,
        AlcanaList,
        UseItem,
        SlotSave,
        LearnSkill,
        ClassChange,
        Rankup,
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
        DungeonMap,
        TutorialStage,
    }

    public class PopupInfo
    {
        public PopupType PopupType;
        public System.Action EndEvent;
        public object template;
    }
}