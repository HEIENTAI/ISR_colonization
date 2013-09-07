using UnityEngine;
using System;
using System.Collections;

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
		
		GameControl.Instance.DebugLog("Loading complete...");
	}

    //SH Add 20130908
    public static BlockGraphicType GetBlockType(ushort typeID)
    {
        return (BlockGraphicType)typeID;
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
			string[] lines = levelFile.text.Split(lineDelimiter, StringSplitOptions.None);
			
			for(int i=0; i< lines.Length; i++)
			{
				string[] blockToken = lines[i].Split(tokenDelimiter, StringSplitOptions.None);
				for(int j=0; j < blockToken.Length; j++)
				{
					generateBlock(blockToken[j], i, j);	
				}
			}
		}
	}
	
	private static void generateBlock(string type, int x, int y)
	{
		GameObject newBlock = null;
        BlockGraphicType graphicType = GetBlockType(Convert.ToUInt16(type));
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
			float xPos = offset_x + Tile_Width*x;
			float yPos = offset_y + Tile_Height*y;
			newBlock.transform.position = new Vector3(xPos, yPos, 0);
			newBlock.transform.parent = root.transform;
		}
	}

}
