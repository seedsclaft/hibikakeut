using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class TutorialDates : ScriptableObject
    {
        [SerializeField] public List<TutorialData> Data = new();
    }

    [Serializable]
    public class TutorialData
    {

        public int Id;
        public Scene SceneType;
        public FrameType Type;
        public int Param1;
        public int Param2;
        public int Param3;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int FocusX;
        public int FocusY;
        public string Name;
        public string Help;
    }

    [Serializable]
    public enum FrameType
    {
        Window = 1,
        Focus = 2,
        Guide = 3,
    }
}