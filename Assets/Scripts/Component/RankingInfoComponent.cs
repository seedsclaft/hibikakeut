using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class RankingInfoComponent : ListItem ,IListViewItem 
    {   
        [SerializeField] private TextMeshProUGUI playerName;
        [SerializeField] private TextMeshProUGUI rank;
        [SerializeField] private TextMeshProUGUI score;
        [SerializeField] private Button detailButton;

        private bool _isInit = false;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<RankingInfo>();
            playerName.text = data.Name;
            score.SetText(data.Score.ToString());
            rank.SetText(DataSystem.GetReplaceText(23030, data.Rank.ToString()));
        }

        public void SetDetailActor(System.Action<List<ActorInfo>> detail)
        {
            if (_isInit == false)
            {
                _isInit = true;
                if (detailButton != null)
                {
                    detailButton.onClick.AddListener(() => 
                    {
                        if (ListData == null) return;
                        var data = ListItemData<RankingInfo>();
                        detail(data.ActorInfos);
                    });
                }
            }
        }
    }
}