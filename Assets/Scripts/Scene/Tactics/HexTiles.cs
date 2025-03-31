using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class HexTiles : BaseList
    {
        private int _lineX = -1;
        private int _lineY = -1;

        private List<HexTileList> _hexTileLists = new();
        
        public void SetLine(int x,int y)
        {
            if (_hexTileLists.Count == 0)
            {
                _hexTileLists = GetComponentsInChildren<HexTileList>().ToList();
            }
            if (_lineX != x || _lineY != y)
            {
                foreach (var _hexTileList in _hexTileLists)
                {
                    _hexTileList.SetSelectIndex(-1);
                }
                _hexTileLists[x].SetSelectIndex(y);
                _lineX = x;
                _lineY = y;
            }
        }
    }
}
