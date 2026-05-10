using UnityEngine;

public class Levitate : MonoBehaviour
{
    [Header("Настройки")]
    public float height = 0.5f;  // Амплитуда (в метрах)
    public float speed = 2f;     // Скорость покачивания

    private Vector3 startPos;

    void Start() => startPos = transform.position;

    void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * height;
        transform.position = startPos + new Vector3(0, offset, 0);
    }
}