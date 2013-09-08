using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator {
	public static int NumOfTiles_X = 10;
	public static int NumOfTiles_Y = 10;
	public static int Tile_Width = 64;
	public static int Tile_Height = 64;
	
	private static int offset_x = -Tile_Width*NumOfTiles_X;
	private static int offset_y = Tile_Height*NumOfTiles_Y;
	
	private static  string[] lineDelimiter = new string[] { "\r\n", "\n" };
	private static  string[] tokenDelimiter = new string[] { "," };
	
	private static GameObject root = null;
    private static GameObject levelGo = null;
	
	private static int extraLineCount = 2;

    private static List<List<MapBlock>> _generatedAllMapData = null;
	public static void Generate(ushort LevelID)
	{
        string fileName = Const.FILENAME_LEVEL_PREFIX + LevelID.ToString();
        GameControl.Instance.StartCoroutine(LoadLevel(fileName));
	}

    private static IEnumerator LoadLevel(string fileName)
	{
		AsyncOperation async = Application.LoadLevelAsync(Const.MAP_NAME_EMPTY);
		yield return async;
		
		generateAllBlocks(fileName);

        GameControl.Instance.TriggerMapGenerateDone();
		GameControl.Instance.DebugLog("Loading complete...");
	}

    //SH Add 20130908
    public static BlockGraphicType GetGraphicBlockType(ushort typeID)
    {
        return (BlockGraphicType)typeID;
    }

    //SH Add 20130908, GraphicBlockType mapping to BlockType
    public static BlockType GetBlockType(BlockGraphicType graphicType)
    {
        switch (graphicType)
        {
            case BlockGraphicType.River_BL:
            case BlockGraphicType.River_BR:
            case BlockGraphicType.River_TL:
            case BlockGraphicType.River_TR:
                return BlockType.River;
            case BlockGraphicType.Sand:
                return BlockType.Sand;
            case BlockGraphicType.Hole:
                return BlockType.Hole;
            case BlockGraphicType.House:
                return BlockType.House;
            case BlockGraphicType.Pyramid:
                return BlockType.Pyramid;
            default:
                return BlockType.Sand;
        }
    }

	private static void generateAllBlocks(string fileName)
	{
		root = GameObject.Find("Blocks");
		if(root == null)
		{
			root = new GameObject("Blocks");
		}

		TextAsset levelFile = Resources.Load(Const.DIR_LEVEL + fileName) as TextAsset;

		if(levelFile != null)
		{
            // sh 20130908 add
            if (_generatedAllMapData != null)
                _generatedAllMapData.Clear();
            _generatedAllMapData = new List<List<MapBlock>>();

			string[] lines = levelFile.text.Split(lineDelimiter, StringSplitOptions.None);
			
			string[] scarabInitPos = lines[lines.Length-extraLineCount].Split(tokenDelimiter, StringSplitOptions.None);
			string[] humanInitPos = lines[lines.Length-extraLineCount+1].Split(tokenDelimiter, StringSplitOptions.None);
			Vector2 scPos = new Vector2( float.Parse(scarabInitPos[0]),float.Parse(scarabInitPos[1]));
			Vector2 humanPos = new Vector2(float.Parse(humanInitPos[0]),float.Parse(humanInitPos[1]));

			BlockGraphicType graphicType;
            MapBlock block = null;
            LivingObject living = null;
            List<MapBlock> oneBlockCol = null;

			for(int i=0; i< lines.Length-extraLineCount; i++)
			{
				string[] blockToken = lines[i].Split(tokenDelimiter, StringSplitOptions.None);

                // handle a new row
				for(int j=0; j < blockToken.Length; j++)
				{
                    if (_generatedAllMapData.Count <= j)
                    {
                        oneBlockCol = new List<MapBlock>();
                        _generatedAllMapData.Add(oneBlockCol);
                    }
                    graphicType = GetGraphicBlockType(Convert.ToUInt16(blockToken[j]));
                    living = null;

                    block = new MapBlock(); // new block data sh20130908
                    block.Pos.x = j;
                    block.Pos.y = i;
                    block.MapBlockType = GetBlockType(graphicType);
                    block.BlockObject = GenerateBlock(graphicType, block.Pos.x, block.Pos.y);

                    if ((block.Pos.x == scPos.x) && (block.Pos.y == scPos.y))
					{
						block.LivingObject = Creature.Scarab;
                        generateScarab(scPos, out living);
					}
                    else if ((block.Pos.x == humanPos.x) && (block.Pos.y == humanPos.y))
					{
                        block.LivingObject = Creature.People;
                        generateHuman(humanPos, out living);
					}
                    else
                        block.LivingObject = Creature.None;

                    block.CreatureComponent = living;
                    _generatedAllMapData[j].Add(block);
				}
			}
		}
	}

    private static GameObject GenerateBlock(BlockGraphicType graphicType, int x, int y)
	{
		GameObject newBlock = null;

        switch (graphicType)
		{
            case BlockGraphicType.Sand: 
			    newBlock = GameObject.Instantiate(Resources.Load(Const.DIR_Prefab + "Sand")) as GameObject;
			break;
            case BlockGraphicType.River_TL: 
			    newBlock = GameObject.Instantiate(Resources.Load(Const.DIR_Prefab + "River_TL")) as GameObject;
			break;
            case BlockGraphicType.River_TR: 
			    newBlock = GameObject.Instantiate(Resources.Load(Const.DIR_Prefab + "River_TR")) as GameObject;
			break;
            case BlockGraphicType.River_BL: 
			    newBlock = GameObject.Instantiate(Resources.Load(Const.DIR_Prefab + "River_BL")) as GameObject;
			break;
            case BlockGraphicType.River_BR:  
			    newBlock = GameObject.Instantiate(Resources.Load(Const.DIR_Prefab + "River_BR")) as GameObject;
			break;
            case BlockGraphicType.Hole:  
			    newBlock = GameObject.Instantiate(Resources.Load(Const.DIR_Prefab + "Hole")) as GameObject;
			break;
            case BlockGraphicType.House:  
			    newBlock = GameObject.Instantiate(Resources.Load(Const.DIR_Prefab + "House")) as GameObject;
			break;
            case BlockGraphicType.Pyramid:  
			    newBlock = GameObject.Instantiate(Resources.Load(Const.DIR_Prefab + "Pyramid")) as GameObject;
			break;
		}
		
		if(newBlock)
		{
			float xPos = offset_x/2f + Tile_Width*x;
			float yPos = offset_y/2f - Tile_Height*y;

            OTSprite sprite = newBlock.GetComponent<OTSprite>();
            sprite.position = new Vector2(xPos, yPos);
            sprite.size = new Vector2(Tile_Width, Tile_Height);
			newBlock.transform.parent = root.transform;
		}

        return newBlock;
	}

    /// <summary>
    /// 人類感染此區域
    /// </summary>
    public static void HumanInfectBlock(MapBlock block)
    {
        LivingObject living = null;

        if (block.CreatureComponent != null)
        {
            if(block.CreatureComponent.gameObject != null)
                GameObject.DestroyImmediate(block.CreatureComponent.gameObject);
        }
        GameObject go = generateHuman(new Vector2(block.Pos.x, block.Pos.y), out living);
        block.CreatureComponent = living; //反設定, 資料更新
    }

    /// <summary>
    /// 蟲類感染此區域
    /// </summary>
    public static void ScarabInfectBlock(MapBlock block)
    {
        LivingObject living = null;

        if (block.CreatureComponent != null)
        {
            if (block.CreatureComponent.gameObject != null)
                GameObject.DestroyImmediate(block.CreatureComponent.gameObject);
        }
        GameObject go = generateScarab(new Vector2(block.Pos.x, block.Pos.y), out living);
        block.CreatureComponent = living; //反設定, 資料更新
    }

    /// <summary>
    /// 移動一個單位, 並且改變資料
    /// </summary>
    public static void CopyMoveUnit(MapBlock original, MapBlock dest)
    {
        if (dest.LivingObject == Creature.People)
        {
            HumanInfectBlock(dest);
        }
        else if (dest.LivingObject == Creature.Scarab)
        {
            ScarabInfectBlock(dest);
        }

        GameObject.DestroyImmediate(original.CreatureComponent.gameObject);
    }

    /// <summary>
    /// 有反索引 block 資料 問題, 先不要使用
    /// </summary>
    public static void MoveUnit(MapBlock original, MapBlock dest)
    {
        if (original.CreatureComponent == null)
        {
            GameControl.Instance.DebugLog(" original.CreatureComponent == null");
            return;
        }
        if (original.CreatureComponent.gameObject == null)
        {
            GameControl.Instance.DebugLog(" original.CreatureComponent.gameObject == null");
            return;
        }

        float xPos = offset_x / 2f + Tile_Width * dest.Pos.x;
        float yPos = offset_y / 2f - Tile_Height * dest.Pos.y;

        //移動資料到新 block
        dest.CreatureComponent = original.CreatureComponent;
        dest.BlockData.Block.LivingObject = original.BlockData.Block.LivingObject;

        //清除 old block
        original.CreatureComponent.gameObject.transform.position = new Vector3(xPos, yPos, -2);
        original.CreatureComponent.gameObject.transform.localScale = new Vector3(Tile_Width, Tile_Height, 1);
        original.BlockData.Block.LivingObject = Creature.None; //原本的位置變成是 空的
        original.CreatureComponent = null; //原本的位置 creature component 索引變成是 空的

        //todo : 要撥 move 特效加這裡
    }

    private static GameObject generateHuman(Vector2 pos, out LivingObject living)
	{
		float xPos = offset_x/2f + Tile_Width*pos.x;
		float yPos = offset_y/2f - Tile_Height*pos.y;
		GameObject newGo = GameObject.Instantiate(Resources.Load("Prefab/Human")) as GameObject;
		newGo.transform.position = new Vector3(xPos, yPos, -2);
        newGo.transform.localScale = new Vector3(Tile_Width, Tile_Height, 1);
        living = newGo.AddComponent<LivingObject>();

		return newGo;
	}

    private static GameObject generateScarab(Vector2 pos, out LivingObject living)
	{
		float xPos = offset_x/2f + Tile_Width*pos.x;
		float yPos = offset_y/2f - Tile_Height*pos.y;
		GameObject newGo = GameObject.Instantiate(Resources.Load("Prefab/Scarab")) as GameObject;
		newGo.transform.position = new Vector3(xPos, yPos, -2);
        newGo.transform.localScale = new Vector3(Tile_Width, Tile_Height, 1);
        living = newGo.AddComponent<LivingObject>();

		return newGo;
		
	}

    // map layout datas
    public static List<List<MapBlock>> GeneratedData
    {
        get 
        {
            return _generatedAllMapData;
        }
    }

    // map hole blocks
    public static Dictionary<IVector2, IVector2> GeneratedHoleData
    {
        get { return null; } // todo
    }

}
