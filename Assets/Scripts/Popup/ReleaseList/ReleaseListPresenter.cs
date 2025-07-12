using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class ReleaseListPresenter : BasePresenter
    {
        ReleaseListModel _model = null;
        ReleaseListView _view = null;

        private bool _busy = true;
        public ReleaseListPresenter(ReleaseListView view)
        {
            _view = view;
            _model = new ReleaseListModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            CommandRefresh();
            _view.OpenAnimation();
        }

        private void CommandEndOpenAnimation()
        {
            CheckTutorialState();
            _busy = false;
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                switch (viewEvent.ViewCommandType.CommandType)
                {
                    case ReleaseList.CommandType.EndOpenAnimation:
                        CommandEndOpenAnimation();
                        break;
                }
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.ReleaseList)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case ReleaseList.CommandType.DecideBuilding:
                    CommandDecideBuilding((BuildingInfo)viewEvent.Template);
                    break;
            }
        }

        private void CommandDecideBuilding(BuildingInfo buildingInfo)
        {
            // 既に入手済み
            if (_model.PartyInfo.BuildingIds.Contains(buildingInfo.Id.Value))
            {
                return;
            }
            // コスト判定
            if (buildingInfo.Master.Cost > _model.PartyInfo.Currency.Value)
            {
                var confirmInfo2 = new ConfirmInfo(DataSystem.GetText(38020),(a) =>
                {
                });
                confirmInfo2.SetBackEvent(() => {});
                confirmInfo2.SetIsNoChoice(true);
                _view.CommandCallConfirm(confirmInfo2);
                return;
            }
            var confirmInfo = new ConfirmInfo(buildingInfo.Master.Name + DataSystem.GetText(38010), (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    _view.CallSystemCommand(Base.CommandType.ClosePopupAll);

                    var getItemData = new GetItemData
                    {
                        Type = GetItemType.Building,
                        Param1 = buildingInfo.Id.Value
                    };
                    var getItemInfo = new GetItemInfo(getItemData);
                    _model.AddGetItemInfo(getItemInfo);

                    CheckAchievements();
                    CommandRefresh();
                }
                _busy = false;
                _view.SetBusy(false);
            });
            _view.CommandCallConfirm(confirmInfo);
        }

        private void CheckTutorialState(object commandType = null)
        {
        }

        private void CommandRefresh()
        {
            Func<BuildingInfo, bool> enable = (buildingInfo) =>
            {
                // 既に解放済み
                return !_model.PartyInfo.BuildingIds.Contains(buildingInfo.Id.Value);
            };
            _view.SetBuildingList(MakeListDataFunc(_model.BuildingInfos(),0,enable));
        }
    }
}