using System;

namespace Ryneus
{
    [Serializable]
    public class StatusInfo
    {
        public ParameterFloat _hp = new();
        public int Hp => (int)Math.Ceiling(_hp.Value);
        public ParameterFloat _mp= new();
        public int Mp => (int)Math.Ceiling(_mp.Value);
        public ParameterFloat _atk= new();
        public int Atk => (int)Math.Ceiling(_atk.Value);
        public ParameterFloat _def= new();
        public int Def => (int)Math.Ceiling(_def.Value);
        public ParameterFloat _spd = new();
        public int Spd => (int)Math.Ceiling(_spd.Value);
        public void SetParameter(int hp,int mp,int atk,int def,int spd)
        {
            _hp.SetValue(hp);
            _mp.SetValue(mp);
            _atk.SetValue(atk);
            _def.SetValue(def);
            _spd.SetValue(spd);
        }

        public int GetParameter(StatusParamType paramType)
        {
            return paramType switch
            {
                StatusParamType.Hp => Hp,
                StatusParamType.Mp => Mp,
                StatusParamType.Atk => Atk,
                StatusParamType.Def => Def,
                StatusParamType.Spd => Spd,
                _ => 0,
            };
        }
        
        public void AddParameter(StatusParamType paramType,float param)
        {
            switch (paramType)
            {
                case StatusParamType.Hp: _hp.GainValue(param); break;
                case StatusParamType.Mp: _mp.GainValue(param); break;
                case StatusParamType.Atk: _atk.GainValue(param); break;
                case StatusParamType.Def: _def.GainValue(param); break;
                case StatusParamType.Spd: _spd.GainValue(param); break;
            }
        }

        public void AddParameterAll(int param)
        {
            _hp.SetValue(param);
            _mp.SetValue(param);
            _atk.SetValue(param);
            _def.SetValue(param);
            _spd.SetValue(param);
        }

        public void Clear()
        {
            SetParameter(0,0,0,0,0);
        }
    }

    public enum StatusParamType
    {
        Hp = 0,
        Mp,
        Atk,
        Def,
        Spd,
    }
}