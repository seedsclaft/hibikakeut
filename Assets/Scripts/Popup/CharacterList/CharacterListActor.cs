using UnityEngine;

namespace Ryneus
{
    public class CharacterListActor : ListItem ,IListViewItem  
    {   
        [SerializeField] private ActorInfoComponent component;
        
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<ActorInfo>();
            component.UpdateInfo(data,null);
        }
    }
}