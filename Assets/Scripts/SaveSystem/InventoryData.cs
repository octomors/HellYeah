using System;
using System.Collections.Generic;

[Serializable]
public class InventoryData
{
    public List<InventoryItemData> items = new List<InventoryItemData>();
}

[Serializable]
public class InventoryItemData
{
    public string ingredientId;
    public int amount;
}
