using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ryneus
{
    public class UnitInfoListPresenter :BasePresenter
    {
        UnitInfoListModel _model = null;
        UnitInfoListView _view = null;

        private bool _busy = true;
        public UnitInfoListPresenter(UnitInfoListView view)
        {
            _view = view;
            _model = new UnitInfoListModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            //_view.SetHelpInputInfo("CHARACTER_LIST");
            if (_model.IsEdit)
            {
                _view.SetUnitInfoList(MakeListData(_model.GetUnitInfos(),0));
            } else
            {
                _view.SetDepatureUnitInfoList(MakeListData(_model.GetUnitInfos(),0));
            }
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
                    case UnitInfoList.CommandType.EndOpenAnimation:
                        CommandEndOpenAnimation();
                        break;
                }
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.UnitInfoList)
            {
                return;
            }
            UnityEngine.Debug.Log(viewEvent.ViewCommandType.CommandType);
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case UnitInfoList.CommandType.DecideBattlerInfo:
                    CommandDecideBattlerInfo();
                    break;
                case UnitInfoList.CommandType.SelectUnitInfo:
                    CommandSelectUnitInfo();
                    break;
                case UnitInfoList.CommandType.SelectMainBattler:
                    CommandSelectMainBattler();
                    break;
                case UnitInfoList.CommandType.SelectSubBattler:
                    CommandSelectSubBattler();
                    break;
                case UnitInfoList.CommandType.InputSelectUnitInfo:
                    CommandInputSelectUnitInfo();
                    break;
            }
        }

        private void CommandDecideBattlerInfo()
        {
            if (_model.IsEdit)
            {
                _busy = true;
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                var characterListInfo = new CharacterListInfo((int actorId) => 
                {    
                    _model.SwapUnitInfos(actorId);
                    _view.SetUnitInfoList(MakeListData(_model.GetUnitInfos(),0));
            
                    _busy = false;
                },
                () => 
                {
                    _busy = false;
                });
                characterListInfo.SetActorInfos(_model.StageMembers());
                
                var popupInfo = new PopupInfo
                {
                    PopupType = PopupType.CharacterList,
                    template = characterListInfo,
                    EndEvent = () =>
                    {
                        _busy = false;
                    }
                };
                _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
            } else
            {
                var select = _view.SelectUnitInfoList;
                if (select != null)
                {
                    _model.CallDecideEvent(select);
                    _view.CallSystemCommand(Base.CommandType.ClosePopup);
                }
            }
        }

        private void CommandSelectUnitInfo()
        {
            var selectBattlerInfo = _view.SelectBattlerInfo();
            _model.SetSelectingBattlerInfo(selectBattlerInfo);
            _view.UnselectAll();
        }

        private void CommandSelectMainBattler()
        {
            _view.CommandSelectMainBattler();
        }

        private void CommandSelectSubBattler()
        {
            _view.CommandSelectSubBattler();
        }

        private void CommandInputSelectUnitInfo()
        {
            var selectBattlerInfo = _view.SelectBattlerInfo();
            _view.CommandInputSelectUnitInfo(selectBattlerInfo);
        }

        private void CheckTutorialState(object commandType = null)
        {
            
        }
    }
}