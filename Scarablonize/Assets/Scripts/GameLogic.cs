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

public class MapBlock
{
	public IVector2 Pos {get;set;}
	public Creature LivingObject {get;set;}
	public BlockType MapBlockType {get;set;}
	
	
	public MapBlock()
	{
		Pos = IVector2.zero;
		LivingObject = Creature.None;
		MapBlockType = BlockType.Sand;
	}
	
	public bool CanMove(Creature creature)
	{
		// 是否有生物在上頭
		if (LivingObject != Creature.None) {return false;}
		// 地形影響
		if (MapBlockType == BlockType.River) {return false;}
		return true;
	}
	
	/// <summary>
	/// 可否被感染成creature
	/// </summary>
	public bool CanInfect(Creature creature)
	{
		if (LivingObject == Creature.None || LivingObject == creature) {return false;}
		// 人在房子內不被感染
		if (LivingObject == Creature.People && MapBlockType == BlockType.House) {return false;}
		return true;
	}
}	

public class Map
{
	const int SIZE_X = 10;
	const int SIZE_Y = 10;
	
	List<List<MapBlock>> allMapBlock;
	
	Dictionary<IVector2, IVector2> holePos;
	
	List<IVector2> peopleCanMovePos;
	List<IVector2> scarabCanMovePos;
	
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
	
	
	
	public Map()
	{
		allMapBlock = new List<List<MapBlock>>();
		holePos = new Dictionary<IVector2, IVector2>();
		peopleCanMovePos = new List<IVector2>();
		scarabCanMovePos = new List<IVector2>();
//		canMoveBlock = 0;
		_peopleCount = 0;
		_scarabCount = 0;
	}
	
	~Map()
	{
		allMapBlock = null;
		holePos = null;
		peopleCanMovePos = null;
		scarabCanMovePos = null;
	}
	
	public void Initialize(List<List<MapBlock>> allMapData, Dictionary<IVector2, IVector2> holeMapData)
	{
		holePos = holeMapData;
        allMapBlock = allMapData;
		
		foreach(List<MapBlock> rowBlockData in allMapData)
		{
			foreach(MapBlock oneBlockData in rowBlockData)
			{
				if (oneBlockData.LivingObject != Creature.None)
				{
					SetCreature(oneBlockData.Pos, oneBlockData.LivingObject);
				}
			}
		}
		
	}
	
	
	/// <summary>
	/// 依據某格的變化,更新可移動區域,
	/// 注意:得先更新該格資訊
	/// </summary>
	void RefreshCanWork(IVector2 pos)
	{
		if (!CheckPosLegal(pos)) {return;}
		if (allMapBlock[pos.x][pos.y].LivingObject == Creature.None)
		{
			for (int diffX = -2; diffX <= 2; ++diffX)
			{
				if (diffX == 0) {continue;}
				if (CheckPosLegal(new IVector2(pos.x + diffX, pos.y)))
				{
					switch(allMapBlock[pos.x + diffX][pos.y].LivingObject)
					{
					case Creature.People:
						if (!peopleCanMovePos.Contains(pos)) {peopleCanMovePos.Add(pos);}
						break;
					case Creature.Scarab:
						if (!scarabCanMovePos.Contains(pos)) {scarabCanMovePos.Add(pos);}
						break;
					}
				}
			}
			for (int diffY = -2; diffY <= 2; ++diffY)
			{
				if (diffY == 0){continue;}
				if (CheckPosLegal(new IVector2(pos.x, pos.y + diffY)))
				{
					switch(allMapBlock[pos.x][pos.y + diffY].LivingObject)
					{
					case Creature.People:
						if (!peopleCanMovePos.Contains(pos)) {peopleCanMovePos.Add(pos);}
						break;
					case Creature.Scarab:
						if (!scarabCanMovePos.Contains(pos)) {scarabCanMovePos.Add(pos);}
						break;
					}
				}
			}
		}
		else
		{
			for (int diffX = -2; diffX <= 2; ++diffX)
			{
				if (diffX == 0) {continue;}
				if (CheckPosLegal(new IVector2(pos.x + diffX, pos.y)))
				{
					switch(allMapBlock[pos.x + diffX][pos.y].LivingObject)
					{
					case Creature.People:
						peopleCanMovePos.Remove(pos);
						break;
					case Creature.Scarab:
						scarabCanMovePos.Remove(pos);
						break;
					}
				}
			}
			for (int diffY = -2; diffY <= 2; ++diffY)
			{
				if (diffY == 0){continue;}
				if (CheckPosLegal(new IVector2(pos.x, pos.y + diffY)))
				{
					switch(allMapBlock[pos.x][pos.y + diffY].LivingObject)
					{
					case Creature.People:
						peopleCanMovePos.Remove(pos);
						break;
					case Creature.Scarab:
						scarabCanMovePos.Remove(pos);
						break;
					}
				}
			}
		}
	}
	
	/// <summary>
	/// 確認是否合法的位置
	/// </summary>
	bool CheckPosLegal(IVector2 pos)
	{
		if (pos.x < 0 || allMapBlock.Count <= pos.x) {return false;}
		if (pos.y < 0 || allMapBlock[pos.x].Count <= pos.y) {return false;}
		return true;
	}
	
	/// <summary>
	/// 判斷該生物是否可以移動
	/// </summary>
	public bool HasMove(Creature creature)
	{
		switch(creature)
		{
		case Creature.People:
			return peopleCanMovePos.Count > 0;
		case Creature.Scarab:
			return scarabCanMovePos.Count > 0;
		}
		return false;
	}
	
	// <summary>
	/// 決定是否還有生物可以移動
	/// </summary>
	public bool HasAnyMove()
	{
		return peopleCanMovePos.Count > 0 || scarabCanMovePos.Count > 0;
	}
	
	public bool CanMove(IVector2 pos, Creature creature)
	{
		if (!CheckPosLegal(pos)) {return false;}
		return allMapBlock[pos.x][pos.y].CanMove(creature);
	}
	
	/// <summary>
	/// 決定可否瞬間移動,可以則transportPos為瞬間移動後的位置
	/// </summary>
	/// <param name="creature">生物種類</param>
	/// <param name="pos">位置</param>
	/// <param name="transportPos">瞬間移動後位置</param>
	public bool CanTransport(Creature creature, IVector2 pos, out IVector2 transportPos) 
	{
		transportPos = pos.Clone();
		if (!CheckPosLegal(pos)) {return false;}
		if (creature != Creature.Scarab) {return false;}
		if (allMapBlock[pos.x][pos.y].MapBlockType != BlockType.Hole) {return false;}
		if (!holePos.ContainsKey(pos)) {return false;}
		else
		{
			transportPos = holePos[pos].Clone();
			if (!CheckPosLegal(transportPos)) {return false;}
			if (allMapBlock[transportPos.x][transportPos.y].LivingObject != Creature.None) {return false;}
			else {return true;}
		}
	}
	
	/// <summary>
	/// 設定pos上面的生物
	/// </summary>
	public void SetCreature(IVector2 pos, Creature creature)
	{
		if (!CheckPosLegal(pos)) {return;}
		switch(allMapBlock[pos.x][pos.y].LivingObject)
		{
		case Creature.People:
			--_peopleCount;
			break;
		case Creature.Scarab:
			--_scarabCount;
			break;
		}
		allMapBlock[pos.x][pos.y].LivingObject = creature;
		switch(allMapBlock[pos.x][pos.y].LivingObject)
		{
		case Creature.People:
			++_peopleCount;
			break;
		case Creature.Scarab:
			++_scarabCount;
			break;
		}
		RefreshCanWork(pos);
	}
	
	/// <summary>
	/// 感染pos上的生物成creature,回傳是否感染成功
	/// </summary>
	public bool Infect(IVector2 pos, Creature creature)
	{
		if (!CheckPosLegal(pos)) {return false;}
		if (!allMapBlock[pos.x][pos.y].CanInfect(creature)) {return false;}

		allMapBlock[pos.x][pos.y].LivingObject = creature;
		return true;
	}
}

public class GameLogic
{
	
	List<IVector2> neighborDir = new List<IVector2>()
	{
		new IVector2( 0, 1), // Up
		new IVector2( 0,-1), // Down
		new IVector2(-1, 0), // Left
		new IVector2( 1, 0), // Right
	};
	
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
	}
	
	~GameLogic()
	{
		map = null;
	}
	
	public void InitialMap(List<List<MapBlock>> allMapData, Dictionary<IVector2, IVector2> holeMapData)
	{
		map.Initialize(allMapData, holeMapData);
	}
	/// <summary>
	/// 檢查生物creature從start到end是否合法
	/// </summary>
	public bool IsLegalMove(IVector2 start, IVector2 end, Creature creature)
	{
		if (map == null) {return false;}
		if (!map.CanMove(end,creature)) {return false;}
		if ((start.x == end.x && start.y != end.y && Mathf.Abs(end.y - start.y) <= 2) ||
			(start.y == end.y && start.x != end.x && Mathf.Abs(end.x - start.x) <= 2)) {return true;}
		return false;
	}
	
	/// <summary>
	/// 處理移動(此處不處理合法性)
	/// </summary>
	/// <param name="start">玩家決定的移動起點</para>
	/// <param name="end">玩家決定的移動終點</param>
	/// <param name="realEnd">實際的移動終點</param>
	/// <param name="infectPositions">感染的座標範圍</param>
	public MoveType Move(Creature creature, IVector2 start, IVector2 end, 
		out IVector2 realEnd, out List<IVector2> infectPositions)
	{
		MoveType moveType = MoveType.None;
		realEnd = end.Clone();
		infectPositions = new List<IVector2>();
		if (creature == Creature.None) {return MoveType.None;}
		if (creature == Creature.People)
		{
			map.SetCreature(realEnd, creature);
			// 由於是合法移動,不是差一就二
			if (Mathf.Abs(realEnd.x - start.x) == 1 || Mathf.Abs(realEnd.y - start.y) == 1) 
			{
				moveType = MoveType.Clone;
			}
			else
			{
				map.SetCreature(start, Creature.None); // 跳耀,所以要刪除原本的
			}
			//感染
			foreach(IVector2 neiDir in neighborDir)
			{
				IVector2 neighbor = new IVector2(realEnd.x + neiDir.x, realEnd.y + neiDir.y);
				if (!neighbor.Equals(start)) // 起點不受影響
				{
					bool infectSuccess = map.Infect(neighbor, creature);
					if (infectSuccess){ infectPositions.Add(neighbor);}
				}
			}
		}
		else if (creature == Creature.Scarab)
		{
			IVector2 transportPos;
			bool isTransPort = map.CanTransport(creature, end, out transportPos);
			if (isTransPort) // 瞬間移動,原本的必定砍掉
			{
				map.SetCreature(start, Creature.None);
				realEnd = transportPos.Clone();
			}
			else
			{
				// 由於是合法移動,不是差一就二
				if (Mathf.Abs(realEnd.x - start.x) == 1 || Mathf.Abs(realEnd.y - start.y) == 1) 
				{
					moveType = MoveType.Clone;
				}
				else
				{
					map.SetCreature(start, Creature.None); // 跳耀,所以要刪除原本的
				}
			}
			map.SetCreature(realEnd, creature);
			//感染
			foreach(IVector2 neiDir in neighborDir)
			{
				IVector2 neighbor = new IVector2(realEnd.x + neiDir.x, realEnd.y + neiDir.y);
				if (!neighbor.Equals(start)) // 起點不受影響
				{
					bool infectSuccess = map.Infect(neighbor, creature);
					if (infectSuccess){ infectPositions.Add(neighbor);}
				}
			}
		}
		
		return moveType;
	}
	/// <summary>
	/// 依據現在盤面決定勝敗
	/// </summary>
	/// <returns>
	/// 勝敗結果
	/// </returns>
	public BattleResult DecideResult(Creature creature)
	{
		if (_peopleCount == 0 && _scarabCount > 0) {return BattleResult.ScarabWin;}
		if (_scarabCount == 0 && _peopleCount > 0) {return BattleResult.PeopleWin;}
		if (!map.HasAnyMove()) // 沒人可移動,比較單位數
		{
			if (_peopleCount > _scarabCount) {return BattleResult.PeopleWin;}
			else if (_peopleCount < _scarabCount) {return BattleResult.ScarabWin;}
			else {return BattleResult.Draw;}
		}
		else // 有人可移動
		{
			if (!map.HasMove(creature)) // 但是creature不能移動
			{
				switch(creature)
				{
				case Creature.People:
					return BattleResult.ScarabWin;
				case Creature.Scarab:
					return BattleResult.PeopleWin;
				}
			}
		}
		return BattleResult.Draw;
	}
}
