using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ruling;

namespace Ryneus
{
    public class RulingView : BaseView
    {
        [SerializeField] private BaseList commandList = null;
        
        [SerializeField] private ToggleSelect toggleSelect = null;
        
        [SerializeField] private BaseList ruleList = null;
        [SerializeField] private TextMeshProUGUI titleText = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        private new System.Action<RulingViewEvent> _commandData = null;

        public override void Initialize() 
        {
            base.Initialize();
            ruleList.Initialize();
            commandList.Initialize();
            toggleSelect.Initialize(new List<string>()
            {
                DataSystem.GetText(102000),
                DataSystem.GetText(102010),
                DataSystem.GetText(102020),
                DataSystem.GetText(102030)
            });
            toggleSelect.SetSelectTabHandler(() => 
            {
                var eventData = new RulingViewEvent(CommandType.SelectCategory);
                var data = toggleSelect.SelectTabIndex;
                eventData.template = data;
                _commandData(eventData);
            });
            toggleSelect.SetSelectTabIndex(0);
            SetBaseAnimation(popupAnimation);
            new RulingPresenter(this);
        }
        
        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform,null);
        }

        public void SetEvent(System.Action<RulingViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetRuleCommand(List<ListData> ruleList)
        {
            commandList.SetData(ruleList);
            commandList.SetInputHandler(InputKeyType.Down,() => CallRulingCommand());
            commandList.SetInputHandler(InputKeyType.Up,() => CallRulingCommand());
            commandList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            commandList.SetSelectedHandler(() => CallRulingCommand());
            SetInputHandler(commandList.gameObject);
        }

        private void CallRulingCommand()
        {
            var listData = commandList.ListData;
            if (listData != null)
            {
                var eventData = new RulingViewEvent(CommandType.SelectRule);
                var data = (SystemData.CommandData)listData.Data;
                eventData.template = data.Id;
                _commandData(eventData);
            }
        }

        public void CommandSelectRule(List<ListData> helpList )
        {
            ruleList.SetData(helpList);
        }

        public void CommandRefresh(List<ListData> helpList)
        {
            commandList.Refresh();
            ruleList.SetData(helpList);
        }
    }
}

namespace Ruling
{
    public enum CommandType
    {
        None = 0,
        SelectRule = 1,
        SelectCategory = 2,
        Back
    }
}

public class RulingViewEvent
{
    public CommandType commandType;
    public object template;

    public RulingViewEvent(CommandType type)
    {
        commandType = type;
    }
}