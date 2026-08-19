using System.Collections;
using System.Collections.Generic;
using System;

namespace Ryneus
{
    public class RulingModel : BaseModel
    {
        private int _currentCategory = 0; 
        public int CurrentCategory => _currentCategory;
        public void SetCategory(int category)
        {
            _currentCategory = category;
            var command = RulingCommand()[0];
            var data = command;
            SetId(data.Id);
        }

        private int _currentId = DataSystem.Dates[DataType.Rules].ToList<RuleData>().Count > 0 ? DataSystem.Dates[DataType.Rules].ToList<RuleData>()[0].Id : 1;

        public void SetId(int id)
        {
            _currentId = id;
        }

        public List<SystemData.CommandData> RulingCommand()
        {
            var list = new List<SystemData.CommandData>();
            foreach (var rule in DataSystem.Dates[DataType.Rules].ToList<RuleData>())
            {
                if (rule.Category == _currentCategory || _currentCategory == 0)
                {
                    SystemData.CommandData ruleCommand = new SystemData.CommandData
                    {
                        Key = rule.Id.ToString(),
                        Id = rule.Id
                    };
                    list.Add(ruleCommand);
                }
            }
            return list;
        }

        public List<string> RuleHelp()
        {
            var helpList = new List<string>();
            var rule = DataSystem.Dates[DataType.Rules].Find<RuleData>(_currentId);
            if (rule != null)
            {
                foreach (var item in rule.Help.Split("\n"))
                {
                    helpList.Add(item);
                }
            }
            return helpList;
        }
    }
}
