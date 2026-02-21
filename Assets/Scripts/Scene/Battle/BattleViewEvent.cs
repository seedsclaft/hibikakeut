namespace Ryneus
{
    namespace Battle
    {
        public enum CommandType
        {
            None = 0,
            Back,
            Escape,
            SelectSideMenu,
            AttributeType,
            StartSelect,
            SkillLog,
            UpdateAp,
            OnSelectSkill,  // 魔法を選択
            OnSelectTarget, // 魔法対象を変更
            OnSelectTargetCursor, // 魔法対象を変更
            OnDecideSkill, // 魔法を決定
            OnDecideEnemy,
            OnCancelEnemy,
            OnDecideActor,
            OnCancelActor,
            EnemyDetail,
            ChangeBattleAuto,
            UpdateBattleAuto,
            ChangeBattleSpeed,
            SkipBattle,
            ForceVictory,
            StopApCount,
            Formation,
            EndFormation,
            SelectCharacter,
            DecideBattle,
        }
    }
}