using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class ListData
    {
        private object _data;
        public object Data => _data;
        public ParameterBool Enable = new(true);
        public ParameterBool Selected = new(false);
        public ParameterBool Batch = new();

        public ListData(object data, bool enable = true)
        {
            _data = data;
            Enable.SetValue(enable);
        }

        public static List<ListData> MakeListData<T>(List<T> dataList)
        {
            var list = new List<ListData>();
            foreach (var data in dataList)
            {
                var listData = new ListData(data);
                listData.Enable.SetValue(true);
                list.Add(listData);
            }
            return list;
        }

        public static List<ListData> MakeListData<T>(List<T> dataList, int selectIndex)
        {
            return MakeListData(dataList, new List<int>() { selectIndex });
        }

        public static List<ListData> MakeListData<T>(List<T> dataList, int selectIndex, Func<T, bool> enable)
        {
            var list = new List<ListData>();
            var idx = 0;
            foreach (var data in dataList)
            {
                var listData = new ListData(data);
                listData.Enable.SetValue(enable(data));
                listData.Selected.SetValue(selectIndex == idx);
                list.Add(listData);
                idx++;
            }
            return list;
        }

        public static List<ListData> MakeListData<T>(List<T> dataList, List<int> selectIndex)
        {
            var list = new List<ListData>();
            var idx = 0;
            foreach (var data in dataList)
            {
                var listData = new ListData(data);
                listData.Enable.SetValue(true);
                if (selectIndex.Contains(idx))
                {
                    listData.Selected.SetValue(true);
                }
                list.Add(listData);
                idx++;
            }
            return list;
        }

        public static List<ListData> MakeListData<T>(List<T> dataList, T selected)
        {
            return MakeListData(dataList, new List<T>() { selected });
        }

        public static List<ListData> MakeListData<T>(List<T> dataList, List<T> selected)
        {
            var list = new List<ListData>();
            foreach (var data in dataList)
            {
                var listData = new ListData(data);
                listData.Enable.SetValue(true);
                if (selected.Contains(data))
                {
                    listData.Selected.SetValue(true);
                }
                list.Add(listData);
            }
            return list;
        }

        public static List<ListData> MakeListData<T>(List<T> dataList, Func<T, bool> enable, Func<T, bool> select)
        {
            var list = new List<ListData>();
            if (enable != null)
            {
                foreach (var data in dataList)
                {
                    var listData = new ListData(data);
                    listData.Enable.SetValue(enable(data));
                    listData.Selected.SetValue(select(data));
                    list.Add(listData);
                }
            }
            return list;
        }

        public static List<ListData> MakeListData<T>(List<T> dataList, bool isEnable = true)
        {
            var list = new List<ListData>();
            foreach (var data in dataList)
            {
                var listData = new ListData(data);
                listData.Enable.SetValue(isEnable);
                list.Add(listData);
            }
            return list;
        }

        public static List<ListData> MakeListData<T>(List<T> dataList, Func<T, bool> enable = null, Func<T, bool> select = null, Func<T, bool> batch = null, int selectIndex = -1)
        {
            var list = new List<ListData>();
            var idx = 0;
            foreach (var data in dataList)
            {
                var listData = new ListData(data);
                if (enable != null)
                {
                    listData.Enable.SetValue(enable(data));
                }
                if (selectIndex >= 0 && idx == selectIndex)
                {
                    listData.Selected.SetValue(true);
                }
                if (select != null)
                {
                    listData.Selected.SetValue(select(data));
                }
                if (batch != null)
                {
                    listData.Batch.SetValue(batch(data));
                }
                list.Add(listData);
                idx++;
            }
            return list;
        }

        public static List<ListData> MakeListData<T>(List<T> dataList, Func<T, bool> enable, int selectIndex = -1)
        {
            var listData = MakeListData(dataList, enable, null, null);
            if (selectIndex != -1 && listData.Count > selectIndex)
            {
                listData[selectIndex].Selected.SetValue(true);
            }
            return listData;
        }
    }
}
