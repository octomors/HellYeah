using UnityEngine;
using System.Collections.Generic;

public class EnemyLoot : MonoBehaviour
{
    [System.Serializable]
    public class LootItem
    {
        public GameObject itemPrefab; // Префаб с компонентом PickableItem
        [Range(0f, 100f)] public float dropChance; // Шанс выпадения в процентах (0 - 100)
        public int minAmount = 1;
        public int maxAmount = 1;
    }

    [Header("Loot Settings")]
    [Tooltip("Список предметов, которые могут выпасть с этого врага")]
    public List<LootItem> lootTable;

    [Header("Spawn Settings")]
    [Tooltip("Максимальное количество предметов, которое может выпасть за раз")]
    public int maxItemsToDrop = 2;
    
    [Tooltip("Небольшое смещение по высоте, чтобы предмет не проваливался в текстуры пола")]
    public float spawnHeightOffset = 0.7f;

    // Метод вызывается из EnemyCombat перед уничтожением врага 
    public void DropLoot()
    {
        if (lootTable == null || lootTable.Count == 0) return;

        int droppedCount = 0;

        // Проходим по списку лута
        foreach (var loot in lootTable)
        {
            if (droppedCount >= maxItemsToDrop) break;

            // Проверяем рандом на шанс выпадения
            float randomRoll = Random.Range(0f, 100f);
            if (randomRoll <= loot.dropChance)
            {
                int finalAmount = Random.Range(loot.minAmount, loot.maxAmount + 1);
                SpawnItem(loot.itemPrefab, finalAmount);
                droppedCount++;
            }
        }
    }

    private void SpawnItem(GameObject prefab, int amount)
    {
        if (prefab == null) return;

        // Задаем радиус разброса лута вокруг врага
        float scatterRadius = 0.5f; 

        // Генерируем случайное смещение по горизонтали (X и Z) внутри круга
        Vector2 randomCircle = Random.insideUnitCircle * scatterRadius;
        Vector3 spawnOffset = new Vector3(randomCircle.x, spawnHeightOffset, randomCircle.y);

        // Итоговая позиция: ноги врага + случайный сдвиг в сторону
        Vector3 finalSpawnPosition = transform.position + spawnOffset;
        
        // Спавним объект
        GameObject spawnedObj = Instantiate(prefab, finalSpawnPosition, Quaternion.identity);

        // Передаем количество в PickableItem
        PickableItem pickable = spawnedObj.GetComponent<PickableItem>();
        if (pickable != null)
        {
            pickable.amount = amount;
        }
    }
}