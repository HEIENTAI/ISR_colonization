using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	Move,   // 移動    
	Clone,  // 需要複製的移動
}

public enum BattleResult 
{
	PeopleWin, // 人類贏了
	ScarabWin, // 甲蟲贏了
	Draw, // 平手
}

public class MapBlock
{
	public IVector2 Pos {get;set;}
	public Creature LivingObject {get;set;}
	public BlockType MapBlockType {get;set;}
	
	public bool CanMove(Creature creature)
	{
		// 是否有生物在上頭
		if (LivingObject != Creature.None) {return false;}
		// 地形影響
		if (MapBlockType == BlockType.River) {return false;}
		return true;
	}
}	

public class Map
{
	const int SIZE_X = 10;
	const int SIZE_Y = 10;
	
	List<List<MapBlock>> allMapBlock;
	
	public Map()
	{
		allMapBlock = new List<List<MapBlock>>();
	}
	
	~Map()
	{
		allMapBlock = null;
	}
	
	public void Initialize()
	{
		for(int x = 0; x < SIZE_X; ++x)
		{
			List<MapBlock> columnMapBlock = new List<MapBlock>();
			allMapBlock.Add(columnMapBlock);
			for (int y = 0; y < SIZE_Y; ++y)
			{
				MapBlock oneMapBlock = new MapBlock();
				oneMapBlock.Pos = new IVector2(x, y);
				oneMapBlock.MapBlockType = BlockType.Sand;
				allMapBlock[x].Add(oneMapBlock);
			}
		}
	}

    public bool CanMove(IVector2 pos, Creature creature)
	{
		if (pos.x < 0 || allMapBlock.Count <= pos.x) {return false;}
		if (pos.y < 0 || allMapBlock[pos.x].Count <= pos.y) {return false;}
		return allMapBlock[pos.x][pos.y].CanMove(creature);
	}
}

public class GameLogic
{
	private int _peopleCount;
	public int PeopleCount
	{
		get {return _peopleCount;} 
	}
	private int _scarabCount;
	public int ScarabCount
	{
		get {return _scarabCount;}
	}
	
	private Map map;
	
	public GameLogic()
	{
		_peopleCount = 0;
		_scarabCount = 0;
		map = new Map();
		// temp
		map.Initialize();
	}
	
	~GameLogic()
	{
		map = null;
	}
	
	/// <summary>
	/// 檢查生物creature從start到end是否合法
	/// </summary>
	public bool IsLegalMove(IVector2 start, IVector2 end, Creature creature)
	{
		if (map == null) {return false;}
		if (!map.CanMove(end,creature)) {return false;}
		
		return true;
	}
	
	/// <summary>
	/// 處理移動
	/// </summary>
	/// <param name="end">
	/// 玩家決定的移動終點
	/// </param>
	/// <param name="realEnd">
	/// 實際的移動終點
	/// </param>
	public MoveType Move(Creature creature, IVector2 end, IVector2 startPos, out IVector2 realEnd)
	{
		realEnd = new IVector2(0,0);
		return MoveType.Move;
	}
	/// <summary>
	/// 依據現在盤面決定勝敗
	/// </summary>
	/// <returns>
	/// 勝敗結果
	/// </returns>
	public BattleResult DecideResult()
	{
		return BattleResult.Draw;
	}
}
