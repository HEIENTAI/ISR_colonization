using UnityEngine;
using System.Collections;

//遊戲進行狀態
public enum PlayStatus
{
    GameTitle,
    GameChooseChapter,
    GameChoosePlaymode,
    MapGenerating, // Map is generating
    RoundScarabTurn,
    ScarabTurnAnimating, // 蟲族方動畫撥放中, 無法操作
    RoundHumanTurn,
    HumanTurnAnimating, // 人類方動畫撥放中, 無法操作
    BattleResult
}

public enum PlayMode
{
    SinglePlay,
    TwoPlayer
}

//遊戲邏輯主控台, 各系統溝通銜接口
public class GameControl{
    public static GameControl _instance;
    private GameMain _main;
    private PlayStatus _currentPlayStatus;
    private PlayMode _currentPlayMode;
    private ushort chapterID = 0; // 0 = no chapter
    // private GUI Manager 預定地, for Sadwx

    private GameControl(GameMain main)
    {
        // GUIManager.ShowGameTitle
        _main = main;
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
    }

    // 玩家從 關卡選擇 UI 畫面, 觸發遊戲開始
    public void TriggerChooseChapter(ushort chapterID)
    {
        if (_currentPlayStatus != PlayStatus.GameChooseChapter)
        {
            DebugLog("Status error.");
            return;
        }

        _currentPlayStatus = PlayStatus.RoundScarabTurn; //蟲族先攻

        // NGUI show ui
    }

    // UI 控制區 (NGUI)

    // Scene (tile, Orthella)

    // update per-frame
    public void Update()
    {
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
