using UnityEngine;
using System;
using System.Collections;

public class MapGenerator {
	public static int NumOfTiles_X = 10;
	public static int NumOfTiles_Y = 10;
	public static int Tile_Width = 64;
	public static int Tile_Height = 64;
	
	private static  string[] lineDelimiter = new string[] { "\r\n", "\n" };
	private static  string[] tokenDelimiter = new string[] { "," };
	
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
		
	}
}
