using System;
using System.Collections;
using System.Collections.Generic;
using Ryneus.EquipmentDetail;

namespace Ryneus
{
    public class EquipmentDetailPresenter : BasePresenter
    {
        EquipmentDetailModel _model = null;
        EquipmentDetailView _view = null;

        private bool _busy = true;
        public EquipmentDetailPresenter(EquipmentDetailView view)
        {
            _view = view;

            SetView(_view);
            _view.SetEvent((type) => UpdateCommand(type));
            Initialize(true);
        }

        private void Initialize(bool first)
        {
            _model = new EquipmentDetailModel();
            SetModel(_model);
            _view.OpenAnimation(first ? InitializeAfter : null);
            if (!first)
            {
                InitializeAfter();
            }
        }

        private void InitializeAfter()
        {
            _view.SetEquipmentDetail(MakeListData(_model.EquipmentInfos(), 0), true);
            _view.SetTitle(_model.SceneParam.Title.Value);
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.EquipmentDetail)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.Initialize:
                    Initialize(false);
                    break;
                case CommandType.DetailItem:
                    CommandDetailEquipment((EquipmentInfo)viewEvent.Template);
                    break;
            }
        }

        private void CommandDetailEquipment(EquipmentInfo equipmentInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _busy = true;
            CommandDetailEquipment(equipmentInfo, () =>
            {
                _busy = false;
            });
        }
    }
}