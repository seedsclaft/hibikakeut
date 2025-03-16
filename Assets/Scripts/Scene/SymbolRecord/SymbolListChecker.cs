using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class SymbolListChecker : SingletonMonoBehaviour<SymbolListChecker>
    {
        [SerializeField] private List<SymbolInfo> symbolInfos = null;
        
        public void SetModel(List<SymbolInfo> model)
        {
            symbolInfos = model;
        }
    }
}
