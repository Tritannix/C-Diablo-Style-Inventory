using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlotSpawner : MonoBehaviour
{
    public GameObject slot;
    //public int inventoryWidth;
    //public int inventoryHeight;

	float tileSize = 64;

	int currentWidth = 0;

	private void Awake()
	{
		//CreateInventorySlots(4, 4);
	}

	public void CreateInventorySlots(int width, int height)
	{
		if (currentWidth == width)
		{
			return;
		}
		foreach (Transform t in transform)
		{
			if (t.gameObject.activeSelf == true)
			{
				Destroy(t.gameObject);
			}
		}

		currentWidth = width;
		
		int xCoord = 0;
		int yCoord = 0;

		float xOffset = transform.position.x - (width * tileSize / 2);

		for (int i = 0; i < height; i++)
		{
			for (int ii = 0; ii < width; ii++)
			{
				GameObject g = Instantiate(slot, transform);
				g.GetComponent<InventorySlot>().pos = new Vector2Int(xCoord, yCoord);
				g.transform.position = new Vector3(xOffset + (xCoord * tileSize),
												   transform.position.y - (yCoord * tileSize),
												   0);
				g.SetActive(true);
				xCoord++;
			}
			xCoord = 0;
			yCoord++;
		}
	}
}
