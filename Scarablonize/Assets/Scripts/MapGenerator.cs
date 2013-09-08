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
	private static int offset_y = -Tile_Height*NumOfTiles_Y;
	
	private static  string[] lineDelimiter = new string[] { "\r\n", "\n" };
	private static  string[] tokenDelimiter = new string[] { "," };
	
	private static GameObject root = null;
    private static GameObject levelGo = null;

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

        levelGo = GameObject.Find("CurrentLevel");

        if (levelGo == null)
        { 
            levelGo = new GameObject("CurrentLevel");
            levelGo.AddComponent<GameLevel>();
        }

		TextAsset levelFile = Resources.Load(Const.DIR_LEVEL + fileName) as TextAsset;

		if(levelFile != null)
		{
            // sh 20130908 add
            if (_generatedAllMapData != null)
                _generatedAllMapData.Clear();
            _generatedAllMapData = new List<List<MapBlock>>();

			string[] lines = levelFile.text.Split(lineDelimiter, StringSplitOptions.None);
            BlockGraphicType graphicType;
            MapBlock block = null;
            List<MapBlock> oneBlockRow = null;
			for(int i=2; i< lines.Length; i++)
			{
                oneBlockRow = new List<MapBlock>();
				string[] blockToken = lines[i].Split(tokenDelimiter, StringSplitOptions.None);

                // handle a new row
				for(int j=0; j < blockToken.Length; j++)
				{
                    graphicType = GetGraphicBlockType(Convert.ToUInt16(blockToken[j]));

                    block = new MapBlock(); // new block data sh20130908
                    block.Pos.x = i;
                    block.Pos.y = j;
                    block.LivingObject = Creature.None;
                    block.MapBlockType = GetBlockType(graphicType);
                    oneBlockRow.Add(block);

                    generateBlock(graphicType, block.Pos.x, block.Pos.y);	
				}
                _generatedAllMapData.Add(oneBlockRow);
			}
		}
	}

    private static void generateBlock(BlockGraphicType graphicType, int x, int y)
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
			float yPos = offset_y/2f + Tile_Height*y;

            OTSprite sprite = newBlock.GetComponent<OTSprite>();
            sprite.position = new Vector2(xPos, yPos);
            sprite.size = new Vector2(Tile_Width, Tile_Height);
			newBlock.transform.parent = root.transform;
		}
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
