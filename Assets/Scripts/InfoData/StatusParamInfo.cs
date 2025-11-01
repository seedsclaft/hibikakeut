using System;

namespace Ryneus
{
    [System.Serializable]
    public class StatusParamInfo
    {
        public StatusParamInfo(StatusParamType statusParamType)
        {
            paramType = statusParamType;
        }
        public StatusParamType paramType = StatusParamType.Hp;
        public ParameterFloat curernt = new();
        public int StatusCurernt => (int)Math.Ceiling(curernt.Value);
        public ParameterFloat max = new();
    }
}
