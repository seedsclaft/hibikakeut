using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class ListData
    {    
        private int _index;
        public int Index => _index;
        private object _data;
        public object Data => _data;
        private bool _enable = true;
        public bool Enable => _enable;
        public void SetEnable(bool enable)
        {
            _enable = enable;
        }
        private bool _selected = false;
        public bool Selected => _selected;
        public void SetSelected(bool selected)
        {
            _selected = selected;
        }

        public ListData(object data,int index = 0,bool enable = true)
        {
            _data = data;
            _index = index;
            _enable = enable;
        }

        public static List<ListData> MakeListData<T>(List<T> dataList)
        {
            var list = new List<ListData>();
            var idx = 0;
            foreach (var data in dataList)
            {
                var listData = new ListData(data,idx);
                listData.SetEnable(true);
                list.Add(listData);
                idx++;
            }
            return list;
        }

        public static List<ListData> MakeListData<T>(List<T> dataList,int selectIndex)
        {
            return MakeListData(dataList,new List<int>(){selectIndex});
        }

        public static List<ListData> MakeListData<T>(List<T> dataList,List<int> selectIndex)
        {
            var list = new List<ListData>();
            var idx = 0;
            foreach (var data in dataList)
            {
                var listData = new ListData(data,idx);
                listData.SetEnable(true);
                if (selectIndex.Contains(idx))
                {
                    listData.SetSelected(true);
                }
                list.Add(listData);
                idx++;
            }
            return list;
        }

        public static List<ListData> MakeListData<T>(List<T> dataList,T selected)
        {
            return MakeListData(dataList,new List<T>(){selected});
        }

        public static List<ListData> MakeListData<T>(List<T> dataList,List<T> selected)
        {
            var list = new List<ListData>();
            var idx = 0;
            foreach (var data in dataList)
            {
                var listData = new ListData(data,idx);
                listData.SetEnable(true);
                if (selected.Contains(data))
                {
                    listData.SetSelected(true);
                }
                list.Add(listData);
                idx++;
            }
            return list;
        }

        public static List<ListData> MakeListData<T>(List<T> dataList,Func<T,bool> enable,Func<T,bool> select)
        {
            var list = new List<ListData>();
            var idx = 0;
            if (enable != null)
            {
                foreach (var data in dataList)
                {
                    var listData = new ListData(data,idx);
                    listData.SetEnable(enable(data));
                    listData.SetSelected(select(data));
                    list.Add(listData);
                    idx++;
                }
            }
            return list;
        }

        public static List<ListData> MakeListData<T>(List<T> dataList,bool isEnable = true)
        {
            var list = new List<ListData>();
            var idx = 0;
            foreach (var data in dataList)
            {
                var listData = new ListData(data,idx);
                listData.SetEnable(isEnable);
                list.Add(listData);
                idx++;
            }
            return list;
        }

        public static List<ListData> MakeListData<T>(List<T> dataList,Func<T,bool> enable = null)
        {
            var list = new List<ListData>();
            var idx = 0;
            if (enable != null)
            {
                foreach (var data in dataList)
                {
                    var listData = new ListData(data,idx);
                    listData.SetEnable(enable(data));
                    list.Add(listData);
                    idx++;
                }
            }
            return list;
        }        
        
        public static List<ListData> MakeListData<T>(List<T> dataList,Func<T,bool> enable,int selectIndex = -1)
        {
            var listData = MakeListData(dataList,enable);
            if (selectIndex != -1 && listData.Count > selectIndex)
            {
                listData[selectIndex].SetSelected(true);
            }
            return listData;
        }
    }
}
