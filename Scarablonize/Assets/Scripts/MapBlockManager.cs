using UnityEngine;
using System.Collections;

public class MapBlock
{
    // Data about
    public IVector2 Pos { get; set; }
    public Creature LivingObject { get; set; }
    public BlockType MapBlockType { get; set; }

    // Visual about
    private GameObject _blockObject = null;
    public GameObject BlockObject {
        get
        {
            return _blockObject;
        }

        set
        {
            _blockObject = value;
            if (_blockObject == null)
            {
                GameControl.Instance.DebugLog("Block gameobject is null.");
                return;
            }

            OTSprite sprite = BlockOTSprite;
            if (sprite == null)
            {
                GameControl.Instance.DebugLog("Block OTSprite is null.");
                return;
            }

            sprite.onInput = MapBlockManager.OnBlockInput;
        }
    }

    public OTSprite BlockOTSprite
    {
        get
        {
            if (_blockObject == null)
            {
                GameControl.Instance.DebugLog("Block gameobject is null.");
                return null;
            }
            OTSprite sprite = _blockObject.GetComponentInChildren<OTSprite>();
            return sprite;
        }
    }
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
        if (LivingObject != Creature.None) { return false; }
        // 地形影響
        if (MapBlockType == BlockType.River || MapBlockType == BlockType.Pyramid) { return false; }
        // 蟲不得進入房子
        if (creature == Creature.Scarab && MapBlockType == BlockType.House) { return false; }
        return true;
    }

    /// <summary>
    /// 可否被感染成creature
    /// </summary>
    public bool CanInfect(Creature creature)
    {
        if (LivingObject == Creature.None || LivingObject == creature) { return false; }
        // 人在房子內不被感染
        if (LivingObject == Creature.People && MapBlockType == BlockType.House) { return false; }
        return true;
    }

    public override string ToString()
    {
        return string.Format("[MapBlock: Pos={0}, LivingObject={1}, MapBlockType={2}]", Pos, LivingObject, MapBlockType);
    }

    public string Name
    {
        get { return BlockObject.name; }
    }
}

/// <summary>
/// Map tiles manage
/// </summary>
public class MapBlockManager {

    public MapBlockManager()
    {
    }

    public static void OnBlockInput(OTObject owner)
    {
        // check for the left mouse button
        if (Input.GetMouseButtonDown(0))
            Debug.Log("Left Button clicked On " + owner.name);
    }

    ~MapBlockManager()
    {
    }
}
