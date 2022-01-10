using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlot : MonoBehaviour
{
	[HideInInspector] public SpriteRenderer sprite;
	public bool isOccupied = false;
	public Vector2Int pos;

	Color primaryColor = Color.white;

	private void Awake()
	{
		sprite = GetComponent<SpriteRenderer>();
		PuzzleLoader.onUnloadPuzzle += Unload;
	}
	
	/// <summary>
	/// Temporarily changes the color of a slot. Call RevertToPrimaryColor to revert.
	/// </summary>
	public void SetTemporaryColor(Color c) { sprite.color = c; }

	/// <summary>
	/// Sets the base color of the tile, either White (empty) or Gray (Occupied).
	/// </summary>
	public void SetPrimaryColor(Color c) { primaryColor = c; }

	/// <summary>
	/// Reverts the slot to its primary color.
	/// </summary>
	public void RevertToPrimaryColor() { sprite.color = primaryColor; }

	void Unload()
	{
		SetPrimaryColor(Color.white);
		if (sprite != null)
		{
			RevertToPrimaryColor();
		}
		isOccupied = false;
	}
}