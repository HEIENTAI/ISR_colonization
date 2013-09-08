using UnityEngine;
using System.Collections;

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
    }

    //------------  Map 控制相關 -------------------
    // player click a tile in Map, Top Left is 0, 0
    public void MapTileClick(float x, float y)
    {
        switch(_currentPlayStatus)
        {
            case PlayStatus.RoundHumanTurn:
            case PlayStatus.RoundScarabTurn:
            if ((_currentPlayStatus != PlayStatus.RoundHumanTurn) &&
                    (_currentPlayStatus != PlayStatus.RoundScarabTurn))
                {
                    DebugLog(" Status error : not round turn status. " + _currentPlayStatus.ToString() );
                    return;
                }

                //_logic.CanControl();
                //_logic.IsLegalMove();
                //_logic.Move();

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
