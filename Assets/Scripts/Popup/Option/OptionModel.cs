using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class OptionModel : BaseModel
    {
        public OptionModel()
        {

        }

        public List<OptionInfo> OptionCommandData(int categoryIndex,System.Action<float> sliderEvent,System.Action<bool> muteEvent,System.Action<int> toggleEvent,System.Action<int> plusMinusEvent)
        {
            var list = new List<OptionInfo>();
            foreach (var optionCommand in DataSystem.OptionCommand)
            {
                if (optionCommand.Category != categoryIndex)
                {
                    continue;
                }
#if UNITY_ANDROID
                if (optionCommand.ExistAndroid == false)
                {
                    continue;
                }
#endif
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
                if (optionCommand.ExistWindows == false)
                {
                    continue;
                }
#endif
                var optionInfo = new OptionInfo
                {
                    OptionCommand = optionCommand,
                    SliderEvent = sliderEvent,
                    MuteEvent = muteEvent,
                    ToggleEvent = toggleEvent,
                    PlusMinusEvent = plusMinusEvent,
                };
                list.Add(optionInfo);
            }
            return list;
        }

        public List<SystemData.CommandData> OptionCategoryList()
        {
            var categoryIds = new List<int>();
            foreach (var optionCommand in DataSystem.OptionCommand)
            {
                if (!categoryIds.Contains(optionCommand.Category))
                {
                    categoryIds.Add(optionCommand.Category);
                }
            }
            var commandDates = new List<SystemData.CommandData>();
            foreach (var categoryId in categoryIds)
            {
                var Command = new SystemData.CommandData
                {
                    Key = categoryId.ToString(),
                    Name = DataSystem.GetText(categoryId + 6000),
                    Id = categoryId
                };
                commandDates.Add(Command);
            }

            return commandDates;
        }

        public int OptionIndex(OptionCategory optionCategory)
        {
            return (int)optionCategory;
        }

        public void ChangeTempInputType(InputType inputType)
        {
            TempInfo.SetInputType(inputType);
        }

    }

    public class OptionInfo
    {
        public SystemData.OptionCommand OptionCommand;
        public InputKeyType keyType;
        public System.Action<float> SliderEvent;
        public System.Action<bool> MuteEvent;
        public System.Action<int> ToggleEvent;
        public System.Action<int> PlusMinusEvent;
    }

    public enum OptionCategory
    {
        System = 0,
        Tactics,
        Battle,
        Data
    }

    public enum OptionButtonType
    {
        None = 0,
        Slider,
        Toggle,
        Button,
        Resolution,
    }
}