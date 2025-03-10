using UnityEngine;
using System.Collections.Generic;

namespace Ryneus
{
    public class BattleChecker : SingletonMonoBehaviour<BattleChecker>
    {
        [SerializeField] private bool forceVictory = false;
        [SerializeField] private bool stopApCount = false;
        [SerializeField] private bool restartApCount = false;
        [SerializeField] private List<BattlerInfo> actorInfos = null;
        [SerializeField] private List<BattlerInfo> enemyInfos = null;

        private BattleModel _model;
        private BattleView _view;
        public void SetModel(BattleModel model,BattleView view)
        {
            _model = model;
            _view = view;
            if (_model != null)
            {
                actorInfos = _model.BattlerActors();
                enemyInfos = _model.BattlerEnemies();
            } else
            {
                actorInfos.Clear();
                enemyInfos.Clear();
            }
        }

        private void Update() 
        {
            if (_view != null)
            {
                if (forceVictory)
                {
                    forceVictory = false;
                    _view.CallViewEvent(Battle.CommandType.ForceVictory);
                }
                if (stopApCount)
                {
                    stopApCount = false;
                    _view.CallViewEvent(Battle.CommandType.StopApCount,true);
                }
                if (restartApCount)
                {
                    restartApCount = false;
                    _view.CallViewEvent(Battle.CommandType.StopApCount,false);
                }
            }    
        }
    }
}
