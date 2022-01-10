using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTile : MonoBehaviour
{
    public Item parentItem;
    public LayerMask slotMask;
    [HideInInspector] public InventorySlot closestInventorySlot;
    public bool canPlace;

	private void Awake()
	{
		Item.endDrag += Item_endDrag;
	}

	private void Item_endDrag()
	{
		if (closestInventorySlot != null)
			ClearCollider();
	}

	void Update()
	{
		if (!parentItem.isDragging)
			return;

		Collider2D c = Physics2D.OverlapPoint(transform.position, slotMask, -Mathf.Infinity, Mathf.Infinity);
		if (c == null)
		{
			canPlace = false;
			if (closestInventorySlot != null)
				ClearCollider();
			return;
		}
		if (closestInventorySlot != null && closestInventorySlot != c)
			closestInventorySlot.RevertToPrimaryColor();

		closestInventorySlot = c.gameObject.GetComponent<InventorySlot>();
		closestInventorySlot.SetTemporaryColor(parentItem.colorTile);
		if (closestInventorySlot.isOccupied)
			canPlace = false;
		else
			canPlace = true;
	}

	void ClearCollider()
	{
		closestInventorySlot.RevertToPrimaryColor();
		closestInventorySlot = null;
    }

	public Vector3 GetDistanceFromCollider()
	{
		return closestInventorySlot.transform.position - transform.position;
	}
}