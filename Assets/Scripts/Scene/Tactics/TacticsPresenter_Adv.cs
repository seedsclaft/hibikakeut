using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public partial class TacticsPresenter : BasePresenter
    {
        
        private void CheckStageEvent()
        {
            // イベントチェック
            var stageEvents = _model.StageEvents(EventTiming.StartTactics);
            foreach (var stageEvent in stageEvents)
            {
                if (_eventBusy)
                {
                    continue;
                }
                switch (stageEvent.Type)
                {
                    case StageEventType.CommandDisable:
                        //_model.SetTacticsCommandEnables((TacticsCommandType)stageEvent.Param + 1,false);
                        break;
                    case StageEventType.NeedAllTactics:
                        break;
                    case StageEventType.IsSubordinate:
                        break;
                    case StageEventType.IsAlcana:
                        //_model.SetIsAlcana(stageEvent.Param == 1);
                        break;
                    case StageEventType.SelectAddActor:
                        
                        break;
                    case StageEventType.SaveCommand:
                        _eventBusy = true;
                        _model.AddEventReadFlag(stageEvent);
                        CommandSave(true);
                        break;
                    case StageEventType.SetDefineBossIndex:
                        break;
                    case StageEventType.SetRouteSelectParam:
                        _view.CallSystemCommand(Base.CommandType.SetRouteSelect);
                        break;
                    case StageEventType.ClearStage:
                        _eventBusy = true;
                        _model.AddEventReadFlag(stageEvent);
                        _view.CommandGotoSceneChange(Scene.MainMenu);
                        break;
                    case StageEventType.ChangeRouteSelectStage:
                        _eventBusy = true;
                        _model.AddEventReadFlag(stageEvent);
                        _view.CommandGotoSceneChange(Scene.Tactics);
                        break;
                    case StageEventType.SetDisplayTurns:
                        break;
                    case StageEventType.MoveStage:
                        break;
                    case StageEventType.SetDefineBoss:
                        //_model.SetDefineBoss(stageEvent.Param);
                        break;
                    case StageEventType.SurvivalMode:
                        break;
                }
            }
        }


        private bool CheckBeforeTacticsAdvEvent()
        {
            var isAbort = CheckAdvStageEvent(EventTiming.BeforeTactics,() => 
            {
                _view.CommandGotoSceneChange(Scene.Tactics);
            },-1);
            if (isAbort)
            {
                _view.gameObject.SetActive(false);
            }
            return isAbort;
        }

    }
}
