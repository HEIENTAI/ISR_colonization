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
		switch(type)
		{
			case "0": 
			    newBlock = GameObject.Instantiate(Resources.Load("Prefabs/Sand")) as GameObject;
			break;
			case "1": 
			    newBlock = GameObject.Instantiate(Resources.Load("Prefabs/River_TL")) as GameObject;
			break;
			case "2": 
			    newBlock = GameObject.Instantiate(Resources.Load("Prefabs/River_TR")) as GameObject;
			break;
			case "3": 
			    newBlock = GameObject.Instantiate(Resources.Load("Prefabs/River_BL")) as GameObject;
			break;
			case "4": 
			    newBlock = GameObject.Instantiate(Resources.Load("Prefabs/River_BR")) as GameObject;
			break;
			case "5": 
			    newBlock = GameObject.Instantiate(Resources.Load("Prefabs/Hole")) as GameObject;
			break;
			case "6": 
			    newBlock = GameObject.Instantiate(Resources.Load("Prefabs/House")) as GameObject;
			break;
			case "7": 
			    newBlock = GameObject.Instantiate(Resources.Load("Prefabs/Pyramid")) as GameObject;
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
