using System;
using System.Collections;
using System.Collections.Generic;
using Ryneus.SelectEquipment;

namespace Ryneus
{
    public class SelectEquipmentPresenter : BasePresenter
    {
        SelectEquipmentModel _model = null;
        SelectEquipmentView _view = null;

        private bool _busy = true;
        public SelectEquipmentPresenter(SelectEquipmentView view)
        {
            _view = view;

            SetView(_view);
            _view.SetEvent((type) => UpdateCommand(type));
            Initialize(true);
        }

        private void Initialize(bool first)
        {
            _model = new SelectEquipmentModel();
            SetModel(_model);
            _view.OpenAnimation(first ? InitializeAfter : null);
            if (!first)
            {
                InitializeAfter();
            }
        }

        private void InitializeAfter()
        {
            CommandRefresh();
            _view.ActivateSelectEquipment(true);
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.SelectEquipment)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.Initialize:
                    Initialize(false);
                    break;
                case CommandType.DecideItem:
                    CommandDecideItem((EquipmentInfo)viewEvent.Template);
                    break;
            }
        }

        private void CommandDecideItem(EquipmentInfo equipmentInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.SelectEquipment(equipmentInfo.Master.Id);
            UpdateEquipmentList();
            if (_model.SelectedEquipment())
            {
                _view.ActivateSelectEquipment(false);
                _busy = true;
                CallConfirmView(DataSystem.GetText(45010), (a) =>
                {
                    if (a == ConfirmCommandType.Yes)
                    {
                        _model.DecideEquipmentInfos();
                        _view.BackEvent.Invoke();
                        return;
                    }
                    _busy = false;
                    _view.ActivateSelectEquipment(true);
                });
            }
        }

        private void CommandRefresh()
        {
            Func<EquipmentInfo, bool> enable = (equipmentInfo) =>
            {
                // 選択可能か
                return !_model.PartyInfo.EquipmentIds.Contains(equipmentInfo.EquipmentId.Value);
            };
            Func<EquipmentInfo, bool> selected = (equipmentInfo) =>
            {
                // 選択中
                return _model.SelectEquipments.Contains(equipmentInfo.EquipmentId.Value);
            };
            _view.SetSelectEquipment(MakeListData(_model.EquipmentInfos(), enable, selected, null, 0), _model.SelectEquipments);
            _view.CheckItemDetailButtonActive();
        }

        private void UpdateEquipmentList()
        {
            Func<EquipmentInfo, bool> enable = (equipmentInfo) =>
            {
                // 選択可能か
                return !_model.PartyInfo.EquipmentIds.Contains(equipmentInfo.EquipmentId.Value);
            };
            Func<EquipmentInfo, bool> selected = (equipmentInfo) =>
            {
                // 選択中
                return _model.SelectEquipments.Contains(equipmentInfo.EquipmentId.Value);
            };
            _view.UpdateEquipmentList(MakeListData(_model.EquipmentInfos(), enable, selected, null, _view.SelectIndex), _model.SelectEquipments);
            //_view.CheckItemDetailButtonActive();
        }

        private void CheckTutorialState(object commandType = null)
        {
        }
    }
}