namespace Utage
{
    public class AdvCustomCommand : AdvCustomCommandManager
    {
        public override void OnBootInit()
        {
            AdvCommandParser.OnCreateCustomCommandFromID += CreateCustomCommand;
        }

        //AdvEnginのクリア処理のときに呼ばれる
        public override void OnClear()
        {
        }

        //カスタムコマンドの作成用コールバック
        public void CreateCustomCommand(string id, StringGridRow row, AdvSettingDataManager dataManager, ref AdvCommand command )
        {
            switch (id)
            {
                //新しい名前のコマンドを作る
                case "PlayBgm":
                    command = new AdvCommandPlayBgm(row);
                    break;
                case "PlayBgs":
                    command = new AdvCommandPlayBgs(row);
                    break;
                case "PlaySe":
                    command = new AdvCommandPlaySe(row);
                    break;
                case "StopBgm2":
                    command = new AdvCommandStopBgm2(row);
                    break;
                case "StopBgs":
                    command = new AdvCommandStopBgs(row);
                    break;
                case "SetSelect1Actor":
                    command = new AdvCommandSetSelect1Actor(row);
                    break;
                case "Balloon":
                    command = new AdvCommandBalloon(row);
                    break;
                case "MoveToTargetDirection":
                    command = new AdvCommandMoveToTargetDirection(row);
                    break;
                case "RotateToCamera":
                    command = new AdvCommandRotateToCamera(row);
                    break;
                case "ShowEventActor":
                    command = new AdvCommandShowEventActor(row);
                    break;
                case "HideEventActor":
                    command = new AdvCommandHideEventActor(row);
                    break;
                case "AnimationEventActor":
                    command = new AdvCommandAnimationEventActor(row);
                    break;
                case "MoveEventActor":
                    command = new AdvCommandMoveEventActor(row);
                    break;
                case "FadeEventActor":
                    command = new AdvCommandFadeEventActor(row);
                    break;
                case "FlipEventActor":
                    command = new AdvCommandFlipEventActor(row);
                    break;
                case "ActiveDungeon":
                    command = new AdvCommandActiveDungeon(row);
                    break;
                case "ResumeDungeonBgm":
                    command = new AdvCommandResumeDungeonBgm(row);
                    break;
                case "BgLoopAnimation":
                    command = new AdvCommandBgLoopAnimation(row);
                    break;
            }
        }
    }
}
