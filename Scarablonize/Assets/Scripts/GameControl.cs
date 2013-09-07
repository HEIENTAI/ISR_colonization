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

//遊戲邏輯主控台
public class GameControl{
    public static GameControl _instance;
    private PlayStatus _currentPlayStatus;

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

    // UI 控制區 (NGUI)

    // Scene (tile, Orthella)

    public void DebugLog(string msg)
    {
        Debug.Log(msg);
    }
}
