using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//遊戲邏輯主控台, 各系統溝通銜接口
public class GameControl{
    private static GameControl _instance;
    private GameMain _main;
    private GameLogic _logic;
    private PlayStatus _currentPlayStatus;
	private PlayStatus _lastPlayStatus;
    private PlayMode _currentPlayMode;
    private ushort _currentChapterID = 0; // 0 = no chapter
	private bool _statusChanged;
    private MapBlockData _currentChoosedBlock = null; // null 表示現在沒有選取 操作中 block 
	private MapBlockData _currentSelection = null;

	public delegate void OnStatusChanged(PlayStatus lastStatus, PlayStatus currentStatus);
	public OnStatusChanged NotifyStatusChanged;

    // private GUI Manager 預定地, for Sadwx

    private GameControl(GameMain main)
    {
        // GUIManager.ShowGameTitle
        _main = main;
        _logic = new GameLogic();

        TriggerGameEnter();
    }

    public static GameControl Instance
    {
        get
        {
            if (_instance == null)
            {
                GameMain main = GameObject.FindObjectOfType(typeof(GameMain)) as GameMain;
                _instance = new GameControl(main);
            }

            return _instance;
        }
    }

    public PlayStatus GameStatus
    {
        get
        {
            return _currentPlayStatus;
        }
    }

    public PlayMode GameMode
    {
        get
        {
            return _currentPlayMode;
        }
    }

    public GameLogic Logic
    {
        get{ return _logic;}
    }

    // 玩家從 title 畫面, 觸發遊戲開始
    public void TriggerGameEnter()
    {
        if(_currentPlayStatus != PlayStatus.GameTitle)
        {
            DebugLog("Status error.");
            return;
        }

        _currentPlayStatus = PlayStatus.GameChoosePlaymode;

        // NGUI show ui
		UIManager.Instance.Open(EnumType.UIType.Menu);
    }

    // 玩家從 mode 選擇觸發
    public void TriggerChoosePlayMode(PlayMode mode)
    {
        if (_currentPlayStatus != PlayStatus.GameChoosePlaymode)
        {
            DebugLog("Status error.");
            return;
        }

        _currentPlayMode = mode;
        _currentPlayStatus = PlayStatus.GameChooseChapter;

        // NGUI show ui
		UIManager.Instance.Open(EnumType.UIType.Levels);
    }

    // 玩家從 關卡選擇 UI 畫面, 觸發遊戲開始
    public void TriggerChooseChapter(ushort chapterID)
    {
        if (_currentPlayStatus != PlayStatus.GameChooseChapter)
        {
            DebugLog("Status error.");
            return;
        }

        _currentChapterID = chapterID;
        _currentPlayStatus = PlayStatus.MapGenerating;

        MapGenerator.Generate( _currentChapterID );

        // NGUI show ui
		UIManager.Instance.Open(EnumType.UIType.InGame);
    }

    // Map 產生完畢後呼叫, PS: 因為 load level 是 async, 所以 is done 由 map generator 判定
    public void TriggerMapGenerateDone()
    {
        if (_currentPlayStatus != PlayStatus.MapGenerating)
        {
            DebugLog("Status error. Should be MapGenerating");
            return;
        }

        DebugLog("蟲族先攻");

        _logic.InitialMap(MapGenerator.GeneratedData, MapGenerator.GeneratedHoleData);
        _currentPlayStatus = PlayStatus.RoundScarabTurn; //蟲族先攻

        DebugLog("PlayStatus: " + _currentPlayStatus.ToString());
    }

    /// <summary>
    /// 現在上場的打擊者
    /// </summary>
    public Creature NowHitter
    {
        get
        {
            if(_currentPlayStatus == PlayStatus.RoundHumanTurn)
                return Creature.People;
            else if(_currentPlayStatus == PlayStatus.RoundScarabTurn)
                return Creature.Scarab;
            else
                return Creature.None;
        }
    }
    //------------  Map 控制相關 -------------------
    // player click a tile in Map, Top Left is 0, 0
    public void MapTileClick(MapBlockData data)
    {
        switch(_currentPlayStatus)
        {
            // 開始選擇要控制哪個 UNIT
            case PlayStatus.RoundHumanTurn:
            case PlayStatus.RoundScarabTurn:
                if (NowHitter == Creature.None)
                {
                    DebugLog(" Status error : not round turn status. " + _currentPlayStatus.ToString() );
                    return;
                }

				UIManager.Instance.ScarabCount = _logic.ScarabCount;				UIManager.Instance.HumanCount = _logic.PeopleCount;                IVector2 vec = new IVector2();                vec.x = data.Column;                vec.y = data.Row;                ControlMessage controlMsg = _logic.CanControl(vec, NowHitter);                if (controlMsg != ControlMessage.OK)                {
					UIManager.Instance.ShowCenterMsg("you can't do it !");                    return;                }
                // todo: some click effect			if(_currentSelection != null)
			    _currentSelection.Block.CreatureComponent.UnHighLight();

                // ready click 2                if (NowHitter == Creature.People)
                {                    _currentPlayStatus = PlayStatus.RoundHumanReadyMove;                }                else if (NowHitter == Creature.Scarab)                {
                    _currentPlayStatus = PlayStatus.RoundScarabReadyMove;
                }
                _currentChoosedBlock = data;
				_currentSelection = _currentChoosedBlock;
			    _currentSelection.Block.CreatureComponent.HighLight();

                DebugLog("PlayStatus: " + _currentPlayStatus.ToString());
                break;

            // 開始選擇要移動到哪裡
            case PlayStatus.RoundHumanReadyMove:
            case PlayStatus.RoundScarabReadyMove:
			
            //debug
            //StartCoroutine(WaitReturnToMain()); //準備回到主畫面

			if(_currentSelection != null)
			    _currentSelection.Block.CreatureComponent.UnHighLight();
			
                //檢查是否為重新選取 unit
                if ((data.Block.LivingObject == Creature.People ) && (_currentPlayStatus == PlayStatus.RoundHumanReadyMove))
                {
                    _currentPlayStatus = PlayStatus.RoundHumanTurn; // 回到選取狀態
                    MapTileClick(data);
                    return;
                }
                else if ((data.Block.LivingObject == Creature.Scarab) && (_currentPlayStatus == PlayStatus.RoundScarabReadyMove))
                {
                    _currentPlayStatus = PlayStatus.RoundScarabTurn; // 回到選取狀態
                    MapTileClick(data);
                    return;
                }

                bool legal = _logic.IsLegalMove(_currentChoosedBlock.Block.Pos, data.Block.Pos);
                if (legal == false)
                {
                    UIManager.Instance.ShowCenterMsg("wrong move position !");
                    return;
                }

                List<IVector2> infectPositions = new List<IVector2>();
                IVector2 realEnd;
                MoveType moveType = _logic.Move(_currentChoosedBlock.Block.Pos, data.Block.Pos, out realEnd, out infectPositions);
                bool isHoleTeleport = ((data.Block.Pos.x != realEnd.x) && (data.Block.Pos.y != realEnd.y));

                DebugLog(" Move Start : " + _currentChoosedBlock.Block.Pos.x.ToString() + "," + _currentChoosedBlock.Block.Pos.y.ToString() +
                         "    End: " + data.Block.Pos.x.ToString() + "," + data.Block.Pos.y.ToString() +
                         "   Real End " + realEnd.x.ToString() + "," + realEnd.y.ToString() + "   MoveType : " + moveType.ToString());

                if (moveType == MoveType.Move)
                {
                    //移動場景物件
                    MapBlock destBlockMove = _logic.GetMapBlock(realEnd);
                    MapGenerator.CopyMoveUnit(_currentChoosedBlock.Block, destBlockMove);

                    for (int i = 0; i < infectPositions.Count; i++)
                    {
                        MapBlock block = _logic.GetMapBlock(infectPositions[i]);

                        //人類
                        if (_lastPlayStatus == PlayStatus.RoundHumanReadyMove)
                        {
                            MapGenerator.HumanInfectBlock(block);
                        }
                        //蟲類
                        else
                        {
                            MapGenerator.ScarabInfectBlock(block);
                        }
                    }

                    if(infectPositions.Count > 0)
                        UIManager.Instance.ShowCenterMsg((isHoleTeleport? "[Teleport]":"") + " Infect !!! ");
                    else
                        UIManager.Instance.ShowCenterMsg((isHoleTeleport ? "[Teleport]" : "") + " Move !");

                    DebugLog(" 淫內感染   infect nums. " + infectPositions.Count.ToString());
                }
                else if (moveType == MoveType.Clone)
                {
                    MapBlock destBlock = _logic.GetMapBlock(realEnd);
                    //人類
                    if (_lastPlayStatus == PlayStatus.RoundHumanReadyMove)
                        MapGenerator.HumanInfectBlock(destBlock);
                    //蟲類
                    else
                        MapGenerator.ScarabInfectBlock(destBlock);

                    for (int i = 0; i < infectPositions.Count; i++)
                    {
                        MapBlock blockClone = _logic.GetMapBlock(infectPositions[i]);

                        //人類
                        if (_lastPlayStatus == PlayStatus.RoundHumanReadyMove)
                        {
                            MapGenerator.HumanInfectBlock(blockClone);
                        }
                        //蟲類
                        else
                        {
                            MapGenerator.ScarabInfectBlock(blockClone);
                        }
                    }

                    if (infectPositions.Count > 0)
                        UIManager.Instance.ShowCenterMsg((isHoleTeleport ? "[Teleport]" : "") + " Clone and Infect !!! ");
                    else
                        UIManager.Instance.ShowCenterMsg((isHoleTeleport ? "[Teleport]" : "") + " Clone ! ");

                    DebugLog("MoveType.Clone " + realEnd.DataToString());
                }
                else
                {
                    DebugLog("MoveType.None");
                }

                BattleResult res = BattleResult.None;
                if (_currentPlayStatus == PlayStatus.RoundHumanReadyMove)
                {
                    _currentPlayStatus = PlayStatus.RoundScarabTurn; // 換蟲方
                    res = _logic.DecideResult(Creature.Scarab);
                }
                else if (_currentPlayStatus == PlayStatus.RoundScarabReadyMove)
                {
                    _currentPlayStatus = PlayStatus.RoundHumanTurn; // 換人方
                    res = _logic.DecideResult(Creature.People);
                }

                res = BattleResult.ScarabWin;
                if (res != BattleResult.None)
                {
                    UIManager.Instance.ShowResult(res);
                    _currentPlayStatus = PlayStatus.BattleResult; //本局結束

                    StartCoroutine(WaitReturnToMain()); //準備回到主畫面

                    if(res == BattleResult.Draw)
                    {
                        UIManager.Instance.ShowCenterMsg("Draw !");
                    }
                    else if(res ==BattleResult.PeopleWin)
                    {
                        UIManager.Instance.ShowCenterMsg("Human Win !");
                    }
                    else if (res == BattleResult.ScarabWin)
                    {
                        UIManager.Instance.ShowCenterMsg("Scarab Win !\n\n Colonization Success !!!");
                    }
                }

                _currentChoosedBlock = null;

                break;

            default:
                DebugLog("Click Tile valid. " + _currentPlayStatus.ToString());
                break;
        }
    }

    // update per-frame
    public void Update()
    {
		if (_lastPlayStatus != _currentPlayStatus)
		{
			if (NotifyStatusChanged != null)
				NotifyStatusChanged(_lastPlayStatus, _currentPlayStatus);

			_lastPlayStatus = _currentPlayStatus;
		}

        switch(_currentPlayStatus)
        {
            case PlayStatus.GameChooseChapter:
                //todo
                break;
            case PlayStatus.GameChoosePlaymode:
                //todo
                break;

            case PlayStatus.GameTitle:
                //todo
                break;
            case PlayStatus.MapGenerating:
                //todo
                break;
            default:
                // todo
                break;
        }
    }

    IEnumerator WaitReturnToMain()
    {
        DebugLog("WaitReturnToMain");
        yield return new WaitForSeconds(8.0f);

        ReturnToMain();
        //_currentPlayStatus = PlayStatus.GameTitle;
        //_logic.ClearMap();

        //GameObject blockRoot = GameObject.Find("Blocks");
        //GameObject.DestroyImmediate(blockRoot);
        //TriggerGameEnter();
    }

    public void ReturnToMain()
    {
        _currentPlayStatus = PlayStatus.GameTitle;
        _logic.ClearMap();

        GameObject blockRoot = GameObject.Find("Blocks");
        GameObject.DestroyImmediate(blockRoot);
        TriggerGameEnter();
    }

    public void StartCoroutine(IEnumerator routine)
    {
        _main.StartCoroutine(routine);
    }

    // message for debug use.
    public void DebugLog(string msg)
    {
        Debug.Log(msg);
    }
}
