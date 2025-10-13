using System;

namespace Ryneus
{
    [Serializable]
    public class StatusInfo
    {
        public StatusParamInfo HpParam = new(StatusParamType.Hp);
        public int Hp => (int)Math.Ceiling(HpParam.curernt.Value);

        public StatusParamInfo MpParam = new(StatusParamType.Mp);
        public int Mp => (int)Math.Ceiling(MpParam.curernt.Value);

        public StatusParamInfo AtkParam = new(StatusParamType.Atk);
        public int Atk => (int)Math.Ceiling(AtkParam.curernt.Value);

        public StatusParamInfo DefParam = new(StatusParamType.Def);
        public int Def => (int)Math.Ceiling(DefParam.curernt.Value);

        public StatusParamInfo SpdParam = new(StatusParamType.Spd);
        public int Spd => (int)Math.Ceiling(SpdParam.curernt.Value);

        public StatusParamInfo CostParam = new(StatusParamType.Cost);
        public int Cost => (int)Math.Ceiling(CostParam.curernt.Value);

        public void SetParameter(int hp, int mp, int atk, int def, int spd, int mov, int cost)
        {
            HpParam.curernt.SetValue(hp);
            MpParam.curernt.SetValue(mp);
            AtkParam.curernt.SetValue(atk);
            DefParam.curernt.SetValue(def);
            SpdParam.curernt.SetValue(spd);
            //_mov.SetValue(mov);
            CostParam.curernt.SetValue(cost);
        }

        public void SetParameter(StatusInfo statusInfo)
        {
            HpParam.curernt.SetValue(statusInfo.Hp);
            MpParam.curernt.SetValue(statusInfo.Mp);
            AtkParam.curernt.SetValue(statusInfo.Atk);
            DefParam.curernt.SetValue(statusInfo.Def);
            SpdParam.curernt.SetValue(statusInfo.Spd);
            CostParam.curernt.SetValue(statusInfo.Cost);
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
                //StatusParamType.Mov => Mov,
                StatusParamType.Cost => Cost,
                _ => 0,
            };
        }

        public void AddParameter(StatusParamType paramType, float param)
        {
            switch (paramType)
            {
                case StatusParamType.Hp: HpParam.curernt.GainValue(param); break;
                case StatusParamType.Mp: MpParam.curernt.GainValue(param); break;
                case StatusParamType.Atk: AtkParam.curernt.GainValue(param); break;
                case StatusParamType.Def: DefParam.curernt.GainValue(param); break;
                case StatusParamType.Spd: SpdParam.curernt.GainValue(param); break;
                //case StatusParamType.Mov: _mov.curernt.GainValue(param); break;
                case StatusParamType.Cost: CostParam.curernt.GainValue(param); break;
            }
        }

        public void SetValue(StatusParamType paramType, float param)
        {
            switch (paramType)
            {
                case StatusParamType.Hp: HpParam.curernt.SetValue(param); break;
                case StatusParamType.Mp: MpParam.curernt.SetValue(param); break;
                case StatusParamType.Atk: AtkParam.curernt.SetValue(param); break;
                case StatusParamType.Def: DefParam.curernt.SetValue(param); break;
                case StatusParamType.Spd: SpdParam.curernt.SetValue(param); break;
                //case StatusParamType.Mov: _mov.curernt.SetValue(param); break;
                case StatusParamType.Cost: CostParam.curernt.SetValue(param); break;
            }
        }

        public void Clear()
        {
            SetParameter(0, 0, 0, 0, 0, 0, 0);
        }
    }

    public enum StatusParamType
    {
        Hp = 0,
        Mp,
        Atk,
        Def,
        Spd,
        Mov,
        Cost
    }
}