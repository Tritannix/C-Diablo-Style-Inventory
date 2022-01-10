using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Item : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
	public Item_SO item;
	SpriteRenderer sr;
	public SpriteDropShadow shadow;
	public MapNode node;
	[HideInInspector] public int itemNumber;

	float tileSize = 64;
	Vector2 originalPosition;
	Vector2 previousPosition;

	[HideInInspector] public string itemName = "";
	/// <summary>
	/// 0 = Common, 1 = Rare, 2 = Epic, 3 = Legendary
	/// </summary>
	[HideInInspector] public int itemRarity = 0;
	bool canUpgrade;
	[HideInInspector] public float goldValue;

	List<Vector2Int> coords = new List<Vector2Int>();

	bool isInInventory = false;

	private void Awake()
	{
		sr = GetComponent<SpriteRenderer>();
	}

	private void Start()
	{
		previousPosition = transform.localPosition;
		originalPosition = transform.localPosition;

		SetStatsFromItem();
		if (node.itemOverrideData[itemNumber].itemNameOverride == "Q")
		{
			if (canUpgrade)
			{
				SetRarity();
				SetRandomName();
				SetRandomGoldValue();
			}
			SetMapNodeOverrideValues();
		}
		else
		{
			itemName = node.itemOverrideData[itemNumber].itemNameOverride;
			itemRarity = node.itemOverrideData[itemNumber].itemRarityOverride;
			goldValue = node.itemOverrideData[itemNumber].itemValueOverride;
		}

		GeneratePhysicsHitbox();
	}

	protected virtual void SetStatsFromItem()
	{
		itemName = item.itemName;
		canUpgrade = item.canUpgrade;
		goldValue = item.goldValue;
		sr.sprite = item.sprite;
		shadow.sr.sprite = item.sprite;

		// Tiles
		for (int i = 0; i < item.tiles.rows.Length; i++)
		{
			for (int ii = 0; ii < item.tiles.rows[i].row.Length; ii++)
			{
				if (item.tiles.rows[i].row[ii] == true)
				{
					coords.Add(new Vector2Int(ii, i));
				}
			}
		}
	}

	[HideInInspector] public bool isDragging = false;
	Vector2 offset;
	public void OnPointerDown(PointerEventData eventData)
	{
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			if (isInInventory)
			{
				ResetPosition();
				return;
			}
		}

		BeginDrag();
	}
	private void Update()
	{
		Drag();
	}

	Vector4 draggingColor = new Vector4(1, 1, 1, 0.42f);
	void BeginDrag()
	{
		Cursor.visible = false;
		sr.color = draggingColor;

		float mousePosX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
		float mousePosY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
		Vector2 mousePositionOnBeginDrag = new Vector2(mousePosX, mousePosY);

		offset = mousePositionOnBeginDrag - (Vector2)transform.position;
		isDragging = true;
		previousPosition = transform.position;

		OccupyExistingSlots(false);
		HideTooltip();

		sr.sortingOrder = 100;
		shadow.Enable();

		if (isInInventory)
		{
			InventoryManager.inst.NumberOfItemsInInventory--;
		}
	}
	void Drag()
	{
		if (!isDragging)
		{
			return;
		}

		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector3 targetPosition = new Vector3(mousePos.x - offset.x,
											 mousePos.y - offset.y,
											 0);
		transform.position = targetPosition;

		SetTileColor();

		if (Input.GetMouseButtonUp(0))
			EndDrag();
	}
	public delegate void OnEndDrag();
	public static event OnEndDrag endDrag;

	void EndDrag()
	{
		Place();

		Cursor.visible = true;
		isDragging = false;

		sr.color = Vector4.one;
		sr.sortingOrder = 0;
		shadow.Disable();
	}

	public GameObject tilePrefab;
	List<ItemTile> tiles = new List<ItemTile>();

	void GeneratePhysicsHitbox()
	{
		Color randomColor = Random.ColorHSV(0, 1, 0, 1, 0, 1);

		foreach (Vector2Int v in coords)
		{
			// Generate colliders
			var b = gameObject.AddComponent<BoxCollider2D>();
			b.size = new Vector2(tileSize, tileSize);
			b.offset = new Vector2((v.x * tileSize) + (tileSize / 2), (v.y * -tileSize) - (tileSize / 2));

			// Generate tiles
			GameObject g = Instantiate(tilePrefab, transform);
			g.transform.position = transform.position + new Vector3((v.x * tileSize) + (tileSize / 2), (v.y * -tileSize) - (tileSize / 2), 0);
			ItemTile tile = g.GetComponent<ItemTile>();
			tile.parentItem = this;
			tiles.Add(tile);
			g.SetActive(true);
			if (item.sprite == null)
			{
				var sr = tile.gameObject.GetComponent<SpriteRenderer>();
				sr.enabled = true;
				sr.color = randomColor;
			}
		}
	}

	Color32 colorValid = new Color32(150, 251, 172, 255);
	Color32 colorInvalid = new Color32(204, 107, 104, 255);
	[HideInInspector] public Color colorOccupied = new Color(0.65f, 0.65f, 0.65f, 1);

	[HideInInspector] public Color32 colorTile;

	void SetTileColor()
	{
		int canPlace = CheckForTileCollision();
		colorTile = canPlace == 2 ? colorValid : colorInvalid;
	}
	
	/// <summary>
	/// 0 = Over no inventory tiles; 1 = Cannot place; 2 = Can place
	/// </summary>
	/// <returns></returns>
	int CheckForTileCollision()
	{
		bool canPlace = true;
		int numberOfEmptyTiles = 0;
		foreach (ItemTile i in tiles)
		{
			if (i.closestInventorySlot == null)
			{
				numberOfEmptyTiles++;
				canPlace = false;
			}
			else if (!i.canPlace)
				canPlace = false;
		}
		// If no tiles are over any inventory slots, place the item where it is
		if (numberOfEmptyTiles == tiles.Count)
			return 0;
		else if (!canPlace)
			return 1;
		else
			return 2;
	}

	public List<InventorySlot> slotsOccupied = new List<InventorySlot>();

	void Place()
	{
		int type = CheckForTileCollision();
		if (type == 2) // Place in inventory
		{
			SnapToNearestTile();
		}
		else if (type == 1) // Place back in previous position
		{
			transform.position = previousPosition;
			OccupyExistingSlots(true);
			if (slotsOccupied.Count > 0)
			{
				isInInventory = true;
				InventoryManager.inst.NumberOfItemsInInventory++;
			}
		}
		else // Place at mouse, outside of inventory
		{
			previousPosition = transform.position;
			slotsOccupied.Clear();
			isInInventory = false;
			ShowTooltip();
		}
		endDrag?.Invoke();
	}
	void SnapToNearestTile()
	{
		transform.position += tiles[0].GetDistanceFromCollider() + new Vector3(tileSize / 2, -tileSize / 2);
		slotsOccupied.Clear();
		foreach (ItemTile i in tiles)
		{
			i.closestInventorySlot.isOccupied = true;
			i.closestInventorySlot.SetPrimaryColor(colorOccupied);
			slotsOccupied.Add(i.closestInventorySlot);
		}
		ShowTooltip();

		isInInventory = true;
		InventoryManager.inst.NumberOfItemsInInventory++;
	}

	void OccupyExistingSlots(bool state)
	{
		if (slotsOccupied.Count <= 0)
		{
			return;
		}
		
		foreach (InventorySlot s in slotsOccupied)
		{
			s.isOccupied = state;
			s.SetPrimaryColor(state == true ? colorOccupied : Color.white);
			s.RevertToPrimaryColor();
		}
	}

	void SetRandomName()
	{
		itemName = NameGenerator.GetRandomName(itemRarity, itemName);
	}
	void SetRarity()
	{
		if (!canUpgrade)
			return;

		itemRarity = UtilityManager.GetRarity();
	}
	void SetRandomGoldValue()
	{
		switch (itemRarity)
		{
			case 1: // Rare
				goldValue *= Random.Range(2f, 2.5f);
				break;
			case 2: // Epic
				goldValue *= Random.Range(4f, 4.5f);
				break;
			case 3: // Legendary
				goldValue *= Random.Range(10f, 11f);
				break;
		}
		goldValue = Mathf.RoundToInt(goldValue);
	}

	void SetMapNodeOverrideValues()
	{
		node.itemOverrideData[itemNumber] = new ItemOverrideData(itemName,
																 itemRarity,
																 Mathf.RoundToInt(goldValue));
	}

	public void ResetPosition()
	{
		transform.localPosition = originalPosition;

		if (isInInventory)
		{
			OccupyExistingSlots(false);
			slotsOccupied.Clear();
			isInInventory = false;
			InventoryManager.inst.NumberOfItemsInInventory--;
		}
	}

	//private void OnDestroy()
	//{
	//	OccupyExistingSlots(false);
	//}

	#region Tooltip
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!isDragging)
			ShowTooltip();
	}
	public void OnPointerExit(PointerEventData eventData)
	{
		HideTooltip();
	}

	void ShowTooltip()
	{
		GameManager.inst.tooltip.Activate(transform.localPosition, itemName, goldValue, sr.bounds.size.y);
	}
	void HideTooltip()
	{
		GameManager.inst.tooltip.Deactivate();
	}
	#endregion
}