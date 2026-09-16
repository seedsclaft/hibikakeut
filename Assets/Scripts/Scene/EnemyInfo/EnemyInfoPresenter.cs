using System.Collections;
using System.Collections.Generic;
using EnemyInfo;

namespace Ryneus
{
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
            CommandRefresh();
            _view.SetActiveSelector(_model.EnemyBattlerInfos.Count > 1);
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
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.SelectEnemy:
                    CommandSelectEnemy();
                    break;
                case CommandType.CallMagicList:
                    CommandCallMagicList();
                    break;
                case CommandType.CallConditionList:
                    CommandCallConditionList();
                    break;
                case CommandType.LeftBattler:
                    CommandLeftBattler();
                    break;
                case CommandType.RightBattler:
                    CommandRightBattler();
                    break;
                case CommandType.Back:
                    CommandBack();
                    break;
            }
        }

        private void CommandSelectEnemy()
        {
            CommandRefresh();
        }

        private void CommandCallMagicList()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _view.CallMagicList();
        }

        private void CommandCallConditionList()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _view.CallConditionList();
        }

        private void CommandLeftBattler()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.ChangeBattlerIndex(-1);
            CommandRefresh();
        }

        private void CommandRightBattler()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.ChangeBattlerIndex(1);
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
            _view.SetCondition(MakeListData(_model.SelectCharacterConditions(), 0));
            _view.CommandRefreshStatus(MakeListData(skillInfos, 0), _model.CurrentEnemy);
        }
    }
}