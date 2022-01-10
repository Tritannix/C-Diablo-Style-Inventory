//using UnityEngine;
//using System.Collections;

[System.Serializable]
public class TileData
{
    [System.Serializable]
    public struct rowData
    {
        public bool[] row;
    }

    public rowData[] rows = new rowData[6];
}