using Ryneus;

namespace Utage
{
    public class AdvCommandRotateToCamera : AdvCommand
    {

        public AdvCommandRotateToCamera(StringGridRow row)
            : base(row)
        {
        }

        public override void DoCommand(AdvEngine engine)
        {
            var scene = GameSystem.SceneStackManager.Current;
            if (scene != Scene.Dungeon)
            {
                return;
            }
            GameSystem.Instance.CurrentScene.CallSystemCommand(Ryneus.Base.CommandType.RotateToCamera, null, true);
        }
    }
}
