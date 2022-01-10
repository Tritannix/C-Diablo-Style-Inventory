using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    #region Singleton : inst
    public static InventoryManager inst { get; private set; }

	private void Awake()
    {
        if (inst == null)
        {
            inst = this;
        }
        else
        {
            Destroy(gameObject);
        }

        PuzzleLoader.onUnloadPuzzle += UnloadPuzzle;
    }
    #endregion

    public int numberOfItemsTotal;
    private int _numberOfItemsInInventory;
    public int NumberOfItemsInInventory
	{
        get { return _numberOfItemsInInventory; }
        set 
        {
            _numberOfItemsInInventory = value;

            if (_numberOfItemsInInventory > numberOfItemsTotal)
            {
                _numberOfItemsInInventory = numberOfItemsTotal;
                Debug.LogError("Number of items in inventory higher than max");
            }
            else if (_numberOfItemsInInventory < 0)
            {
                _numberOfItemsInInventory = 0;
                Debug.LogError("Number of items in inventory less than 0");
                return;
            }
            else if (_numberOfItemsInInventory == numberOfItemsTotal)
			{
                CompletePuzzle();
                return;
			}
        }
	}
    
    
    public PuzzleLoader puzzleLoader;

    public void CompletePuzzle()
	{
        GameManager.inst.tooltip.Deactivate();
        numberOfItemsTotal = 0;
        _numberOfItemsInInventory = 0;
        puzzleLoader.CompleteCurrentPuzzle();
	}

    void UnloadPuzzle()
	{
        _numberOfItemsInInventory = 0;
	}



    //public void AddTiles()
    //{
    //       numberOfItemsInInventory++;

    //       if (numberOfItemsInInventory > numberOfItemsTotal)
    //       {
    //           numberOfItemsInInventory = numberOfItemsTotal;
    //           Debug.LogError("Number of items in inventory higher than max");
    //       }
    //   }
    //   public void SubtractTiles()
    //   {
    //       numberOfItemsInInventory--;

    //	if (numberOfItemsInInventory < 0)
    //	{
    //		numberOfItemsInInventory = 0;
    //		Debug.LogError("Number of items in inventory less than 0");
    //	}
    //}
}