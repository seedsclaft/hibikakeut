using Ryneus;

namespace Utage
{
    public class AdvCommandMoveToTargetDirection : AdvCommand
    {
        private int? _direction = -1;

        public AdvCommandMoveToTargetDirection(StringGridRow row)
            : base(row)
        {
            _direction = ParseCell<int>(AdvColumnName.Arg1);
        }

        public override void DoCommand(AdvEngine engine)
        {
            var scene = GameSystem.SceneStackManager.Current;
            if (scene != Scene.Dungeon)
            {
                return;
            }
            GameSystem.Instance.CurrentScene.CallViewEvent(Ryneus.Dungeon.CommandType.MoveDirection, _direction);
        }
    }
}
