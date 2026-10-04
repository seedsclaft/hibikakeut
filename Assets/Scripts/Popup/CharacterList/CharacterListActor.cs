using System;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class CharacterListActor : ListItem, IListViewItem
    {
        [SerializeField] private ActorInfoComponent component;
        [SerializeField] private Button detailButton;

        public void SetDetailEvent(Action<ActorInfo> detailEvent)
        {
            detailButton.onClick.AddListener(() =>
            {
                var data = ListItemData<ActorInfo>();
                detailEvent.Invoke(data);
            });
        }

        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }
            var data = ListItemData<ActorInfo>();
            component.UpdateInfo(data, null);
            UIComponent.SetActive(Disable, !ListData.Enable.Value);
        }
    }
}