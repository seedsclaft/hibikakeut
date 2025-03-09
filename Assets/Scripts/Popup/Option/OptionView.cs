using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Option;
using Ryneus;

namespace Ryneus
{
    public class OptionView : BaseView
    {
        [SerializeField] private BaseList optionCategoryList = null;
        [SerializeField] private BaseList optionList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        public OptionInfo OptionCommandInfo => optionList.ListItemData<OptionInfo>();
        public int OptionCategoryIndex => optionCategoryList.Index;

        public override void Initialize() 
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Option);
            SetBaseAnimation(popupAnimation);
            InitializeCategoryList();
            InitializeOptionList();
            SetActivate(optionCategoryList);
            new OptionPresenter(this);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }
        
        private void InitializeCategoryList()
        {
            optionCategoryList.Initialize();
            optionCategoryList.SetSelectedHandler(() =>
            {
                CallViewEvent(CommandType.SelectCategory);
            });
            optionCategoryList.SetInputHandler(InputKeyType.Decide,() => CallDecideCategory());
            optionCategoryList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            SetInputHandler(optionCategoryList.gameObject);
            AddViewActives(optionCategoryList);
        }

        public void SetOptionCategoryList(List<ListData> optionData)
        {
            optionCategoryList.SetData(optionData);
        }

        private void InitializeOptionList()
        {
            optionList.Initialize();
            optionList.SetInputHandler(InputKeyType.Right,() => CallChangeOptionValue(InputKeyType.Right));
            optionList.SetInputHandler(InputKeyType.Left,() => CallChangeOptionValue(InputKeyType.Left));
            optionList.SetInputHandler(InputKeyType.Option1,() => CallChangeOptionValue(InputKeyType.Option1));
            optionList.SetInputHandler(InputKeyType.Decide,() => OnClickOptionList());
            optionList.SetInputHandler(InputKeyType.Cancel,() => CallCancelOptionList());
            SetInputHandler(optionList.gameObject);
            AddViewActives(optionList);
        }

        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform,null);
        }

        public void SetOptionList(List<ListData> optionData)
        {
            optionList.ResetScrollRect();
            optionList.SetData(optionData);
        }

        private void OnClickOptionList()
        {
            CallViewEvent(CommandType.SelectOptionList);
        }

        public void CommandRefresh()
        {
            if (optionCategoryList.Active)
            {
                SetHelpInputInfo("OPTION_CATEGORY");
            } else
            {
                SetHelpInputInfo("OPTION");
            }
            optionList.UpdateAllItems();
        }

        public void SetHelpWindow()
        {
            SetHelpText(DataSystem.GetHelp(500));
        }

        public new void SetBackEvent(System.Action backEvent)
        {
            SetBackCommand(() => 
            {    
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                GameSystem.OptionData.InputType = GameSystem.TempData.TempInputType;
                if (GameSystem.OptionData.InputType == InputType.MouseOnly)
                {
                    SetHelpInputInfo("");
                }
                if (backEvent != null) backEvent();
            });
            ChangeBackCommandActive(true);
        }

        public void CallChangeOptionValue(InputKeyType inputKeyType)
        {
            var listData = optionList.ListData;
            if (listData == null)
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            var optionResultInfo = (OptionInfo)listData.Data;
            var optionInfo = new OptionInfo
            {
                OptionCommand = optionResultInfo.OptionCommand,
                keyType = inputKeyType
            };
            CallViewEvent(CommandType.ChangeOptionValue,optionInfo);
        }

        private void CallDecideCategory()
        {
            CallViewEvent(CommandType.DecideCategory);
        }
        
        public void DecideCategory()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            SetActivate(optionList);
            optionList.UpdateSelectIndex(0);
        }

        private void CallCancelOptionList()
        {
            CallViewEvent(CommandType.CancelOptionList);
        }

        public void CancelOptionList()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            optionList.UpdateSelectIndex(-1);
            SetActivate(optionCategoryList);
        }
    }
}

namespace Option
{
    public enum CommandType
    {
        None = 0,
        SelectCategory = 101,
        SelectOptionList = 102,
        CancelOptionList = 103,
        DecideCategory = 104,
        ChangeOptionValue = 2000,
    }
}

public class PopupInfo
{
    public PopupType PopupType;
    public System.Action EndEvent;
    public object template;
}