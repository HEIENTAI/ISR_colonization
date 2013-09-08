using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


public static class CommonFunction
{
	public static string DataToString(this IVector2 vec)
	{
		return string.Format("({0}, {1})", vec.x, vec.y);
	}

    public static uint DataToUInt(this IVector2 vec)
    {
        return (uint)vec.x * Const.PosXTOIntTime + (uint)vec.y;
    }
}

public class Map
{
	List<List<MapBlock>> allMapBlock;
	
    //Dictionary<IVector2, IVector2> holePos;
    Dictionary<uint, IVector2> holePos;
	// 生物可移動位置
	Dictionary<Creature, List<IVector2>> _creatureCanMovePos;
	// 生物數量
	Dictionary<Creature, int> _creatureCount;
	
	public int PeopleCount
	{
		get 
		{
			if (_creatureCount == null || !_creatureCount.ContainsKey(Creature.People)) {return 0;}
			return _creatureCount[Creature.People];
		}
	}
	
	public int ScarabCount
	{
		get
		{
			if (_creatureCount == null || !_creatureCount.ContainsKey(Creature.Scarab)) {return 0;}
			return _creatureCount[Creature.Scarab];
		}
	}
	
	public Map()
	{
		allMapBlock = new List<List<MapBlock>>();
        //holePos = new Dictionary<IVector2, IVector2>();
        holePos = new Dictionary<uint, IVector2>();

		_creatureCanMovePos = new  Dictionary<Creature, List<IVector2>>();
		_creatureCanMovePos.Add(Creature.People, new List<IVector2>());
		_creatureCanMovePos.Add(Creature.Scarab, new List<IVector2>());
		_creatureCount = new Dictionary<Creature, int>();
		_creatureCount.Add(Creature.People, 0);
		_creatureCount.Add(Creature.Scarab, 0);
	}
	
	~Map()
	{
        allMapBlock.Clear();
		allMapBlock = null;
		holePos = null;
		_creatureCanMovePos = null;
		_creatureCount = null;
	}

    public void Clear()
    {
        // todo: 應該 check 移除掉 go 還有哪些相關聯的資料需要移除
        if (allMapBlock != null)
        {
            for (int x = 0; x < allMapBlock.Count; ++x)
            {
                for (int y = 0; y < allMapBlock[x].Count; ++y)
                {
                    if (allMapBlock[x][y].CreatureComponent == null)
                        continue;
                    GameObject.DestroyImmediate(allMapBlock[x][y].CreatureComponent.gameObject);
                }
            }

            allMapBlock.Clear();
        }

        // 清除生物 count
        if (_creatureCount != null)
        {
            _creatureCount[Creature.Scarab] = 0;
            _creatureCount[Creature.People] = 0;
        }

        allMapBlock = null;
        if (holePos != null)
         holePos.Clear();
        holePos = null;
    }
	
	public override string ToString ()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendFormat("Map : allMapBlock\n");
		for (int x = 0; x < allMapBlock.Count; ++x)
		{
			for (int y = 0; y <allMapBlock[x].Count; ++y)
			{
				sb.AppendFormat("allMapBlock[{0}][{1}] = {2}\n", x, y, allMapBlock[x][y]);
			}
		}
		if (holePos != null)
		{
			sb.AppendFormat("holePos:\n");
            foreach(KeyValuePair<uint, IVector2> oneHoldPos in holePos)
			{
				sb.AppendFormat("holePos[({0}, {1})] = {2}\n", oneHoldPos.Key / Const.PosXTOIntTime, oneHoldPos.Key % Const.PosXTOIntTime, 
                    oneHoldPos.Value.DataToString());
			}
		}
		sb.AppendFormat("peopleCanMovePos:\n");
		foreach (Creature creature in _creatureCanMovePos.Keys)
		{
            sb.AppendFormat(DebugCanMove(creature));
		}
		foreach(Creature creature in _creatureCount.Keys)
		{
			if (creature != Creature.None)
			{
				sb.AppendFormat("生物({0})的數量 = {1}\n", creature, _creatureCount[creature]);  
			}
		}
		return sb.ToString();
	}

    public string DebugCanMove(Creature creature)
    {
        StringBuilder sb = new StringBuilder();
        if (creature != Creature.None)
        {
            sb.AppendFormat("生物({0})的可移動位置:\n", creature);
            for (int index = 0; index < _creatureCanMovePos[creature].Count; ++index)
            {
                sb.AppendFormat("	pos[{0}] = {1}\n", index, _creatureCanMovePos[creature][index].DataToString());
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// 取得pos位置上面的生物
    /// </summary>
    /// <param name="pos">要知道生物種類的位置</param>
    /// <returns>該位置上的生物種類</returns>
    public Creature GetCreature(IVector2 pos)
    {
        if (!CheckPosLegal(pos)) { return Creature.None; }
        return allMapBlock[pos.x][pos.y].LivingObject;
    }

	public void Initialize(List<List<MapBlock>> allMapData, Dictionary<IVector2, IVector2> holeMapData)
	{
        allMapBlock = allMapData;
        if (holePos == null) { holePos = new Dictionary<uint, IVector2>(); }
        foreach (IVector2 key in holeMapData.Keys)
        {
            holePos.Add(key.DataToUInt(), holeMapData[key]);
        }
		
		foreach(List<MapBlock> rowBlockData in allMapData)
		{
			foreach(MapBlock oneBlockData in rowBlockData)
			{
				if (oneBlockData.LivingObject != Creature.None)
				{
					++_creatureCount[oneBlockData.LivingObject];
					RefreshCanWorkFrom(oneBlockData.Pos);
				}
			}
		}
		
	}

    public MapBlock GetMapBlock(IVector2 pos)
    {
        for (int x = 0; x < allMapBlock.Count; ++x)
        {
            for (int y = 0; y < allMapBlock[x].Count; ++y)
            {
                if((allMapBlock[x][y].Pos.x == pos.x) && (allMapBlock[x][y].Pos.y == pos.y)) //todo: 以後可以改成 x + y 當 key
                {
                    return allMapBlock[x][y];
                }
            }
        }
        return null;
    }
	
	/// <summary>
	/// 將creature的可移動位置增加pos(不做pos檢查)
	/// </summary>
	/// <param name='creature'>要增加可移動位置的生物種類</param>
	/// <param name='pos'>要加的可移動位置</param>
	void AddCanWorkPos(Creature creature, IVector2 pos)
	{
		// 如果判斷是否有creature的可移動位置儲存,沒有就產生一個
		if (!_creatureCanMovePos.ContainsKey(creature))
		{
			_creatureCanMovePos.Add(creature, new List<IVector2>());
		}
		// 是否已經存入此位置,沒有才加入pos
		if (!_creatureCanMovePos[creature].Contains(pos))
		{
			_creatureCanMovePos[creature].Add(pos);
		}
	}
	
	/// <summary>
	/// 將creature的可移動位置刪除pos(不做pos檢查)
	/// </summary>
	/// <param name='creature'>要刪除可移動位置的生物種類</param>
	/// <param name='pos'>要刪除的可移動位置</param>
	void RemoveCanWorkPos(Creature creature, IVector2 pos)
	{
		// 如果判斷是否有creature的可移動位置儲存,沒有就產生一個
		if (!_creatureCanMovePos.ContainsKey(creature))
		{
			_creatureCanMovePos.Add(creature, new List<IVector2>());
		}
		_creatureCanMovePos[creature].Remove(pos);
	}
	
	/// <summary>
	/// 更新某位置能夠可以移動的地方
	/// 注意:得先更新該格資訊
	/// </summary>
	void RefreshCanWorkFrom(IVector2 pos)
	{
		if (!CheckPosLegal(pos)) {return;}
		if (allMapBlock[pos.x][pos.y].LivingObject != Creature.None)
		{
			// 處理此位置移動出去的部分
			for (int diffX = -2; diffX <= 2; ++diffX)
			{
				if (diffX == 0) {continue;}
				IVector2 targetPos = new IVector2(pos.x + diffX, pos.y);
				if (CheckPosLegal(targetPos))
				{	// 若目標位置可移動過去
					if (allMapBlock[targetPos.x][targetPos.y].CanMoveTo(allMapBlock[pos.x][pos.y].LivingObject))
					{
						AddCanWorkPos(allMapBlock[pos.x][pos.y].LivingObject, targetPos);
					}
				}
			}
			for (int diffY = -2; diffY <= 2; ++diffY)
			{
				if (diffY == 0) {continue;}
				IVector2 targetPos = new IVector2(pos.x, pos.y + diffY);
				if (CheckPosLegal(targetPos))
				{	// 若目標位置可移動過去
					if (allMapBlock[targetPos.x][targetPos.y].CanMoveTo(allMapBlock[pos.x][pos.y].LivingObject))
					{
						AddCanWorkPos(allMapBlock[pos.x][pos.y].LivingObject, targetPos);
					}
				}
			}
		}
	}
	
	
	/// <summary>
	/// 依據某格的變化,更新可移動區域,
	/// 注意:得先更新該格資訊
	/// </summary>
	void RefreshCanWorkTo(IVector2 pos)
	{
		if (!CheckPosLegal(pos)) {return;}
		if (allMapBlock[pos.x][pos.y].LivingObject == Creature.None)
		{
			for (int diffX = -2; diffX <= 2; ++diffX)
			{
				if (diffX == 0) {continue;}
				IVector2 changePos = new IVector2(pos.x + diffX, pos.y);
				if (CheckPosLegal(changePos))
				{
					if (allMapBlock[pos.x][pos.y].CanMoveTo(allMapBlock[changePos.x][changePos.y].LivingObject))
					{
						AddCanWorkPos(allMapBlock[changePos.x][changePos.y].LivingObject, pos);
					}
				}
			}
			for (int diffY = -2; diffY <= 2; ++diffY)
			{
				if (diffY == 0){continue;}
				IVector2 changePos = new IVector2(pos.x, pos.y + diffY);
				if (CheckPosLegal(changePos))
				{
					if (allMapBlock[pos.x][pos.y].CanMoveTo(allMapBlock[changePos.x][changePos.y].LivingObject))
					{
						AddCanWorkPos(allMapBlock[changePos.x][changePos.y].LivingObject, pos);
					}
				}
			}
		}
		else
		{
			for (int diffX = -2; diffX <= 2; ++diffX)
			{
				if (diffX == 0) {continue;}
				IVector2 changePos = new IVector2(pos.x +diffX, pos.y);
				if (CheckPosLegal(changePos))
				{
					RemoveCanWorkPos(allMapBlock[changePos.x][changePos.y].LivingObject, pos);
				}
			}
			for (int diffY = -2; diffY <= 2; ++diffY)
			{
				if (diffY == 0){continue;}
				IVector2 changePos = new IVector2(pos.x, pos.y + diffY);
				if (CheckPosLegal(changePos))
				{
					RemoveCanWorkPos(allMapBlock[changePos.x][changePos.y].LivingObject, pos);
				}
			}
		}
	}
	
	/// <summary>
	/// 確認是否合法的位置
	/// </summary>
	bool CheckPosLegal(IVector2 pos)
	{
        if (pos.x < 0 || allMapBlock.Count <= pos.x) { return false; }
		if (pos.y < 0 || allMapBlock[pos.x].Count <= pos.y) {return false;}
		return true;
	}
	
	/// <summary>
	/// 判斷該生物是否可以移動
	/// </summary>
	public bool HasMove(Creature creature)
	{
		if (_creatureCanMovePos == null || !_creatureCanMovePos.ContainsKey(creature)) {return false;}
		return _creatureCanMovePos[creature].Count > 0;
	}
	
	// <summary>
	/// 決定是否還有生物可以移動
	/// </summary>
	public bool HasAnyMove()
	{
		if (_creatureCanMovePos == null) {return false;}
		foreach(List<IVector2> oneCreatureCanMovePos in _creatureCanMovePos.Values)
		{
			if (oneCreatureCanMovePos.Count > 0) {return true;}
		}
		return false;
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
        if (creature != Creature.Scarab || allMapBlock[pos.x][pos.y].MapBlockType != BlockType.Hole) { return false; }
		if (holePos == null || !holePos.ContainsKey(pos.DataToUInt() )) {return false;}
		else
		{
			transportPos = holePos[pos.DataToUInt()].Clone();
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
		allMapBlock[pos.x][pos.y].LivingObject = creature;
		if (_creatureCount.ContainsKey(allMapBlock[pos.x][pos.y].LivingObject))
		{
			++_creatureCount[allMapBlock[pos.x][pos.y].LivingObject];
		}
        RefreshCanWorkFrom(pos);
		RefreshCanWorkTo(pos);
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


    public void RecheckAllCanMove()
    {
        foreach (Creature creature in Enum.GetValues(typeof(Creature)))
        {
            if (creature != Creature.None)
            {
                _creatureCanMovePos[creature].Clear();
            }
        }

        foreach (List<MapBlock> oneCol in allMapBlock)
        {
            foreach (MapBlock block in oneCol)
            {
                if (block.LivingObject == Creature.None)
                {
                    for (int diffx = -2; diffx <= 2; ++diffx)
                    {
                        if (diffx == 0) { continue; }
                        IVector2 posCanMoveToThisPos =  new IVector2(block.Pos.x + diffx, block.Pos.y);
                        if (CheckPosLegal(posCanMoveToThisPos))
                        {
                            if (block.CanMoveTo(Creature.People) && allMapBlock[posCanMoveToThisPos.x][posCanMoveToThisPos.y].LivingObject == Creature.People)
                            {
                                AddCanWorkPos(Creature.People, block.Pos);
                            }
                            else if (block.CanMoveTo(Creature.Scarab) && allMapBlock[posCanMoveToThisPos.x][posCanMoveToThisPos.y].LivingObject == Creature.Scarab)
                            {
                                AddCanWorkPos(Creature.Scarab, block.Pos);
                            }
                        }
                    }
                    for (int diffy = -2; diffy <= 2; ++diffy)
                    {
                        if (diffy == 0) { continue; }
                        IVector2 posCanMoveToThisPos = new IVector2(block.Pos.x, block.Pos.y + diffy);
                        if (CheckPosLegal(posCanMoveToThisPos))
                        {
                            if (block.CanMoveTo(Creature.People) && allMapBlock[posCanMoveToThisPos.x][posCanMoveToThisPos.y].LivingObject == Creature.People)
                            {
                                AddCanWorkPos(Creature.People, block.Pos);
                            }
                            else if (block.CanMoveTo(Creature.Scarab) && allMapBlock[posCanMoveToThisPos.x][posCanMoveToThisPos.y].LivingObject == Creature.Scarab)
                            {
                                AddCanWorkPos(Creature.Scarab, block.Pos);
                            }
                        }
                    }

                }
            }
        }

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
		Debug.Log(map.ToString());
	}

    public void ClearMap()
    {
        map.Clear();
    }

    /// <summary>
    /// 雖然在這裡很奇怪, 取得 view + model reference 共用的地塊資料, MapBlock
    /// </summary>
    public MapBlock GetMapBlock(IVector2 pos)
    {
        return map.GetMapBlock(pos);
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
		MoveType moveType = MoveType.Move;
		realEnd = end.Clone();
		infectPositions = new List<IVector2>();
        Creature creature = map.GetCreature(start); // 取得要移動的生物
		if (creature == Creature.None) {return MoveType.None;}
		if (creature == Creature.People)
		{
			map.SetCreature(realEnd, creature);
            Debug.Log(string.Format(map.DebugCanMove(Creature.People)));
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
        // TODO: 優化能夠移動到的地點相關的處理
        map.RecheckAllCanMove();
        
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
        // 尚未分出勝負
		return BattleResult.None;
	}
}
