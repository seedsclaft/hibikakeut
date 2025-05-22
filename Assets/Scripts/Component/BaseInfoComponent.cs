using UnityEngine;

namespace Ryneus
{
    public class BaseInfoComponent : MonoBehaviour
    {
        public SaveInfo CurrentData => GameSystem.CurrentData;
        public SaveGameInfo CurrentGameInfo => GameSystem.GameInfo;
        public TempInfo TempInfo => GameSystem.TempData;
        public StageInfo CurrentStage => CurrentGameInfo.StageInfo;

        public PartyInfo PartyInfo => CurrentGameInfo.PartyInfo;
        public DeckInfo CurrentDeckInfo => CurrentGameInfo.PartyInfo.CurrentDeckInfo;
    }
}
