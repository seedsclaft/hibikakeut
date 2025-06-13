using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ryneus.ArtifactList;

namespace Ryneus
{
    public class ArtifactListView : BaseView
    {
        [SerializeField] private BaseList artifactList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;

        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.ArtifactList);
            InitializeArtifactList();
            SetBaseAnimation(popupAnimation);
            _ = new ArtifactListPresenter(this);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform, () => {});
        }

        private void InitializeArtifactList()
        {
            artifactList.Initialize();
            artifactList.SetInputHandler(InputKeyType.Cancel, () => BackEvent());
            SetInputHandler(artifactList.gameObject);
        }

        public void SetArtifactList(List<ListData> achievementLists)
        {
            artifactList.SetData(achievementLists);
            artifactList.Activate();
        }
    }

    namespace ArtifactList
    {
        public enum CommandType
        {
        }
    }
}
