using UnityEngine;
using System.Collections;

//遊戲進行狀態
public enum PlayStatus
{
    GameTitle,
    GameChooseChapter,
    GameChoosePlaymode,
    RoundHumanTurn,
    HumanTurnAnimating, // 人類方動畫撥放中, 無法操作
    RoundScarabTurn,
    ScarabTurnAnimating // 蟲族方動畫撥放中, 無法操作
}

public enum PlayMode
{
    SinglePlay,
    TwoPlayer
}

//遊戲邏輯主控台
public class GameControl{
    public static GameControl _instance;
    private PlayStatus _currentPlayStatus;
    private PlayMode _currentPlayMode;
    // private GUI Manager 預定地, for Sadwx

    public GameControl()
    {
    }

    public static GameControl Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameControl();

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

        _currentPlayStatus = PlayStatus.GameChooseChapter;

        // NGUI show ui
    }

    // 玩家從 title 畫面, 觸發遊戲開始
    public void TriggerChooseChapter()
    {
        if (_currentPlayStatus != PlayStatus.GameTitle)
        {
            DebugLog("Status error.");
            return;
        }

        _currentPlayStatus = PlayStatus.GameChooseChapter;

        // NGUI show ui
    }


    // UI 控制區 (NGUI)

    // Scene (tile, Orthella)

    // update per-frame
    public void Update()
    {

    }

    // message for debug use.
    public void DebugLog(string msg)
    {
        Debug.Log(msg);
    }
}
