using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class Item_SO : ScriptableObject
{
    public string itemName;
    public int goldValue;
    public bool canUpgrade;
    public UpgradeClass upgradeClass;

    public Sprite sprite;

    public TileData tiles;
}

public enum UpgradeClass
{
    NONE,
    SHORTBOW,
    LONGBOW,
    CROSSBOW,
    DAGGER,
    GREATSWORD,
    HANDAXE,
    AXE,
    BUCKLER,
    SHIELD,
    SPELLBOOK,
    SPELLSTAFF,
    SCYTHE,
    HELMET,
    SHOULDERS,
    CHESTPIECE,
    BELT,
    GLOVES,
    TROUSERS,
    BOOTS,
    RING,
    BRACELET,
    AMULET,
    TRINKET,
    SOULGEM
}