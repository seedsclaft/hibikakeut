using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    using EnemyInfo;
    public class EnemyInfoPresenter : BasePresenter
    {
        EnemyInfoModel _model = null;
        EnemyInfoView _view = null;

        private bool _busy = true;
        public EnemyInfoPresenter(EnemyInfoView view)
        {
            _view = view;
            _model = new EnemyInfoModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetHelpWindow();
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetEnemies(MakeListData(_model.EnemyBattlerInfos,0));
            CommandRefresh();
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy /*|| _view.AnimationBusy*/)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Status)
            {
                return;
            }
            UnityEngine.Debug.Log(viewEvent.commandType);
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.SelectEnemy:
                    CommandSelectEnemy();
                    break;
                case CommandType.Back:
                    CommandBack();
                    break;
            }
        }

        private void CommandSelectEnemy()
        {
            var selectIndex = _view.EnemyListIndex;
            _model.SelectEnemyIndex(selectIndex);
            _view.UpdateEnemyList(selectIndex);
            CommandRefresh();
        }

        private void CommandBack()
        {
            _view.CommandBack();
        }

        private void CommandRefresh()
        {
            var skillInfos = _model.SkillActionList();
            var lastSelectIndex = 0;
            _view.SetCondition(MakeListData(_model.SelectCharacterConditions()));
            _view.CommandRefreshStatus(MakeListData(skillInfos),_model.CurrentEnemy,MakeListData(_model.EnemySkillTriggerInfo()),_model.EnemyIndexes(),lastSelectIndex);
        }
    }
}