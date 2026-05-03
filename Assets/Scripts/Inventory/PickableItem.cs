using UnityEngine;

// Вешается на 3D объект ингредиента в сцене
// При нажатии на E добавляет ингредиент в InventoryManager и уничтожает объект
// Требует на объекте: Collider + слой Interactable
[RequireComponent(typeof(Collider))]
public class PickableItem : MonoBehaviour, IInteractable
{
    public Ingredient ingredient;

    [Tooltip("Сколько штук добавить в инвентарь при подборе")]
    public int amount = 1;

    [Header("Покачивание")]
    public bool bobbing = true; //включить/выключить плавное движение вверх-вниз
    public float bobHeight = 0.08f; //амплитуда покачивания (максимальное отклонение от исходной позиции)
    public float bobSpeed  = 1.5f; //скорость

    [Header("Вращение")]
    public bool rotate = true; //вращается ли объект вокруг своей вертикальной оси
    public float rotateSpeed = 45f; //градусы в секунду

    [Header("Outline при наведении")]
    [SerializeField] private Outline _outline;

    private Vector3 _startPos;

    private void Start()
    {
        _startPos = transform.position;
    }

    private void Update()
    {
        if (bobbing)
        {
            float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        if (rotate)
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    // Метод вызывается из системы взаимодействия, когда игрок нажал клавишу E, глядя на этот объект
    public void Interact()
    {
        if (ingredient == null)
        {
            Debug.LogError($"[PickableItem] На объекте '{gameObject.name}' не назначен Ingredient!");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[PickableItem] InventoryManager не найден на сцене!");
            return;
        }

        InventoryManager.Instance.AddIngredient(ingredient, amount);

        if (UIManager.Instance != null)
            UIManager.Instance.ShowTextHint($"Подобрано: {ingredient.ingredientName} ×{amount}");

        Destroy(gameObject);
    }

    public string GetInteractText() =>
        ingredient != null ? $"Подобрать {ingredient.ingredientName} [E]" : "Подобрать [E]";

    public Outline GetOutline() => _outline;
    public Transform GetTransform() => transform;
}