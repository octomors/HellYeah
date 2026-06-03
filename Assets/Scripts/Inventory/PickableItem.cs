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
    private SpriteRenderer _spriteRenderer; //ссылка на компонент отображения картинки

    private void Start()
    {
        _startPos = transform.position;

        // Автоматически ищем SpriteRenderer в дочерних объектах
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Если нашли и ингредиент назначен, подставляем иконку
        if (_spriteRenderer != null && ingredient != null)
        {
            _spriteRenderer.sprite = ingredient.icon; //берем иконку из ScriptableObject

            // Сбрасываем масштаб в дефолтный 1,1,1 для точного расчета
            _spriteRenderer.transform.localScale = Vector3.one;

            // Желаемый размер иконки в игровом мире (например, 0.3 юнита в ширину/высоту)
            float targetSize = 0.3f; 

            // Получаем текущие физические размеры спрайта в мире
            float spriteWidth = _spriteRenderer.bounds.size.x;
            float spriteHeight = _spriteRenderer.bounds.size.y;

            // Высчитываем коэффициент масштабирования, чтобы сохранить пропорции
            float maxDimension = Mathf.Max(spriteWidth, spriteHeight);
            float scaleFactor = targetSize / maxDimension;

            // Применяем одинаковый масштаб по X и Y, чтобы картинку не перекосило
            _spriteRenderer.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        }
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