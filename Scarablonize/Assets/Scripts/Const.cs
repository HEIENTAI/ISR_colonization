public class Const{	public const int MapSize = 10;
    public const string FILENAME_LEVEL_PREFIX = "level"; // levelxxx.txt 的檔名
    public const string MAP_NAME_STARTSCENE = "Main"; //should be useless...
    public const string MAP_NAME_EMPTY = "xLevel";
    public const string DIR_LEVEL = "levels/";
    public const string DIR_Prefab = "Prefab/";
    // Extension name
    public const string EXT_LEVEL_CONFIG = ".txt";}public class EnumType{	public enum UIType	{		Menu,		Levels,		InGame,		Staff	}}

// reference to: BlockType
public enum BlockGraphicType
{
    Sand = 0,
    River_TL = 1,
    River_TR = 2,
    River_BL = 3,
    River_BR = 4,
    Hole = 5,
    House = 6,
    Pyramid = 7
}

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
public enum ShowStatus{	Hide,	BringIn,	BringOut}
public enum Creature
{
    None = 0, // 無
    People = 1, // 人
    Scarab = 2, // 甲蟲
}

public enum BlockType
{
    River = 0, // 河流
    Sand = 1, // 沙漠
    Pyramid = 2, // 金字塔
    Hole = 3, // 蟲洞
    House = 4, // 房子
}

public enum MoveType
{
    None,   // 無
    Move,   // 移動    
    Clone,  // 需要複製的移動
}

public enum BattleResult
{
    PeopleWin, // 人類贏了
    ScarabWin, // 甲蟲贏了
    Draw, // 平手
}

public enum ControlMessage
{
    OK, // 可以移動
    WrongCreature, // 控制的生物種類錯誤
    NoneCreature, // 要控制的位置沒有生物
    PositionError, // 位置錯誤
}public class TagConst{	public const string SinglePlay = "1P";	public const string TwoPlayers = "2P";	public const string Staff = "Staff";	public static readonly string[] Levels = new string[]	{		"Level1",		"Level2",		"Level3",		"Level4"	};}