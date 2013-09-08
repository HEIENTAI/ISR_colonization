using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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
	
    /// <summary>
    /// creature可否移動到此格
    /// </summary>
    /// <param name="creature">要移動的生物</param>
    /// <returns>能否移動</returns>
	public bool CanMoveTo(Creature creature)
	{
		// 是否有生物在上頭
		if (LivingObject != Creature.None) {return false;}
		// 地形影響
		if (MapBlockType == BlockType.River || MapBlockType == BlockType.Pyramid) {return false;}
        // 蟲不得進入房子
		if (creature == Creature.Scarab && MapBlockType == BlockType.House) {return false;}
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
	
	public override string ToString ()
	{
		return string.Format ("[MapBlock: Pos={0}, LivingObject={1}, MapBlockType={2}]", Pos, LivingObject, MapBlockType);
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
	 // 生物數量
	Dictionary<Creature, int> _creatureCount;
	
//	private int _peopleCount;
	public int PeopleCount
	{
//		get {return _peopleCount;}
		get 
		{
			if (_creatureCount == null || !_creatureCount.ContainsKey(Creature.People)) {return 0;}
			return _creatureCount[Creature.People];
		}
	}
	
//	private int _scarabCount;
	public int ScarabCount
	{
//		get {return _scarabCount;}
		get
		{
			if (_creatureCount == null || !_creatureCount.ContainsKey(Creature.Scarab)) {return 0;}
			return _creatureCount[Creature.Scarab];
		}
	}
	
	
	public Map()
	{
		allMapBlock = new List<List<MapBlock>>();
		holePos = new Dictionary<IVector2, IVector2>();
		peopleCanMovePos = new List<IVector2>();
		scarabCanMovePos = new List<IVector2>();
		_creatureCount = new Dictionary<Creature, int>();
		_creatureCount.Add(Creature.People, 0);
		_creatureCount.Add(Creature.Scarab, 0);
//		_peopleCount = 0;
//		_scarabCount = 0;
	}
	
	~Map()
	{
		allMapBlock = null;
		holePos = null;
		peopleCanMovePos = null;
		scarabCanMovePos = null;
		_creatureCount = null;
	}
	
	public override string ToString ()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendFormat("Map : allMapBlock\n");
		int x = 0;
		int y = 0;
		foreach(List<MapBlock> colMapBlock in allMapBlock)
		{
			foreach(MapBlock oneMapBlock  in colMapBlock)
			{
				sb.AppendFormat("allMapBlock[{0}][{1}] = {2}\n", x, y, allMapBlock[x][y]);
				++y;
			}
			++x;
		}
		sb.AppendFormat("holePos:\n");
		foreach(IVector2 startHoldPos in holePos.Keys)
		{
			sb.AppendFormat("holePos[{0}] = {1}\n", startHoldPos, holePos[startHoldPos]);
		}
		sb.AppendFormat("peopleCanMovePos:\n");
		int index = 0;
		foreach(IVector2 peopleOneMovePos in peopleCanMovePos)
		{
			sb.AppendFormat("peopleCanMovePos[{0}] = {1}\n", index, peopleOneMovePos);
			++index;
		}
		index = 0;
		sb.AppendFormat("scarabCanMovePos:\n");
		foreach(IVector2 scarabOneMovePos in scarabCanMovePos)
		{
			sb.AppendFormat("scarabCanMovePos[{0}] = {1}\n", index, scarabCanMovePos);
			++index;
		}
		foreach(Creature creature in _creatureCount.Keys)
		{
			sb.AppendFormat("生物({0})的數量 = {1}", creature, _creatureCount[creature]);  
		}
//		sb.AppendFormat("_peopleCount = {0} _scarabCount = {1}", _peopleCount,_scarabCount);
		return sb.ToString();
	}

    /// <summary>
    /// 取得pos位置上面的生物
    /// </summary>
    /// <param name="pos">要知道生物種類的位置</param>
    /// <returns>該位置上的生物種類</returns>
    public Creature GetCreature(IVector2 pos)
    {
        if (CheckPosLegal(pos)) { return Creature.None; }
        return allMapBlock[pos.x][pos.y].LivingObject;
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

    /// <summary>
    /// 判斷creature回合時，可否控制pos位置的移動
    /// </summary>
    /// <param name="pos">要控制移動的pos</param>
    /// <param name="creature">哪種生物的回合</param>
    /// <returns>控制錯誤訊息</returns>
    public ControlMessage CanControl(IVector2 pos, Creature creature)
    {
        if (!CheckPosLegal(pos)) { return ControlMessage.PositionError; }
        if (allMapBlock[pos.x][pos.y].LivingObject == Creature.None) { return ControlMessage.NoneCreature; }
        if (allMapBlock[pos.x][pos.y].LivingObject != creature) { return ControlMessage.WrongCreature; }
        else { return ControlMessage.OK; }
    }

    /// <summary>
    /// 判斷start的生物可否移動到end
    /// </summary>
    /// <param name="start">移動起點</param>
    /// <param name="end">移動終點</param>
    /// <returns>可否移動</returns>
    public bool CanMove(IVector2 start, IVector2 end)
    {
        if (!CheckPosLegal(start) || !CheckPosLegal(end)) { return false; }
        return allMapBlock[end.x][end.y].CanMoveTo(allMapBlock[start.x][start.y].LivingObject);
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
		if (creature != Creature.Scarab || allMapBlock[pos.x][pos.y].MapBlockType != BlockType.Hole) {return false;}
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
		if (_creatureCount.ContainsKey(allMapBlock[pos.x][pos.y].LivingObject))
		{
			--_creatureCount[allMapBlock[pos.x][pos.y].LivingObject];
		}
//		switch(allMapBlock[pos.x][pos.y].LivingObject)
//		{
//		case Creature.People:
//			--_peopleCount;
//			break;
//		case Creature.Scarab:
//			--_scarabCount;
//			break;
//		}
		allMapBlock[pos.x][pos.y].LivingObject = creature;
		if (_creatureCount.ContainsKey(allMapBlock[pos.x][pos.y].LivingObject))
		{
			++_creatureCount[allMapBlock[pos.x][pos.y].LivingObject];
		}
//		switch(allMapBlock[pos.x][pos.y].LivingObject)
//		{
//		case Creature.People:
//			++_peopleCount;
//			break;
//		case Creature.Scarab:
//			++_scarabCount;
//			break;
//		}
		RefreshCanWork(pos);
	}
	
	/// <summary>
	/// 感染pos上的生物成creature,回傳是否感染成功
	/// </summary>
	public bool Infect(IVector2 pos, Creature creature)
	{
		if (!CheckPosLegal(pos)) {return false;}
		if (!allMapBlock[pos.x][pos.y].CanInfect(creature)) {return false;}

        SetCreature(pos, creature);
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
	
	/// <summary>
	/// 取得人數
	/// </summary>
	public int PeopleCount
	{
		get {return (map == null) ? 0 : map.PeopleCount;}
	}
	/// <summary>
	/// 取得甲蟲數
	/// </summary>
	public int ScarabCount
	{
		get {return (map == null) ? 0 : map.ScarabCount;}
	}
	
	private Map map;
	
	public GameLogic()
	{
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
    /// 檢查在creature回合時，可否控制pos位置的移動 （滑鼠點第一下的檢查）
    /// </summary>
    /// <param name="pos">想檢查控制移動的位置</param>
    /// <param name="creature">現在的回合是哪種生物</param>
    /// <returns></returns>
    public ControlMessage CanControl(IVector2 pos, Creature creature)
    {
        if (map == null) { return ControlMessage.PositionError; }
        return map.CanControl(pos, creature);
    }

    /// <summary>
    /// 檢查start上的生物移動到end是否為合法移動 (滑鼠點擊第二下的檢查）
    /// </summary>
    /// <param name="start">移動起始點</param>
    /// <param name="end">移動終點</param>
    /// <returns>能否移動</returns>
    public bool IsLegalMove(IVector2 start, IVector2 end)
    {
        if (map == null) { return false; }
        if (!map.CanMove(start, end)) { return false; }
        if ((start.x == end.x && start.y != end.y && Mathf.Abs(end.y - start.y) <= 2) ||
            (start.y == end.y && start.x != end.x && Mathf.Abs(end.x - start.x) <= 2)) { return true; }
        return false;
    }

	/// <summary>
	/// 處理移動(檢查都通過時的處理）
	/// </summary>
	/// <param name="start">玩家決定的移動起點</para>
	/// <param name="end">玩家決定的移動終點</param>
	/// <param name="realEnd">實際的移動終點</param>
	/// <param name="infectPositions">感染的座標範圍</param>
	public MoveType Move(IVector2 start, IVector2 end, out IVector2 realEnd, out List<IVector2> infectPositions)
	{
		MoveType moveType = MoveType.None;
		realEnd = end.Clone();
		infectPositions = new List<IVector2>();
        Creature creature = map.GetCreature(start); // 取得要移動的生物
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
		if (map.PeopleCount == 0 && map.ScarabCount > 0) {return BattleResult.ScarabWin;}
		if (map.ScarabCount == 0 && map.PeopleCount > 0) {return BattleResult.PeopleWin;}
		if (!map.HasAnyMove()) // 沒人可移動,比較單位數
		{
			if (map.PeopleCount > map.ScarabCount) {return BattleResult.PeopleWin;}
			else if (map.PeopleCount < map.ScarabCount) {return BattleResult.ScarabWin;}
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
