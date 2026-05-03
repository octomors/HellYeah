using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FlyingIngredient : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private CanvasGroup canvasGroup;
    
    public void Setup(Sprite icon, Vector3 startPosition, Vector3 endPosition, System.Action onComplete)
    {
        // Берем у ингредиента на который нажали спрайт и добавляем в наш ингредиент (который будет лететь к котлу)
        if (iconImage != null)
            iconImage.sprite = icon;
        
        // Получаем RectTransform этого же объекта - начальная позиция полета
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.position = startPosition;
        
        // Вычисляем среднюю точку котла с подъёмом вверх (середина дуги) - берём среднюю точку между началом и концом и поднимаем её вверх на 150 пикселей
        Vector3 midPoint = (startPosition + endPosition) / 2f + Vector3.up * 150f;
        
        // Создаём "очередь анимаций" - DOTween будет проигрывать их в порядке, который мы укажем
        Sequence sequence = DOTween.Sequence();
        
        // DOPath – анимация движения по точкам. PathType.CatmullRom – способ интерполяции кривой. Он делает траекторию плавной, будто точки соединены гибкой проволокой, а не ломаной линией
        // SetEase(Ease.OutQuad) – замедление в конце.
        sequence.Append(rectTransform.DOPath(new Vector3[] { startPosition, midPoint, endPosition }, 0.6f, PathType.CatmullRom)
            .SetEase(Ease.OutQuad));
        // Join – анимация проигрывается одновременно с предыдущей. То есть масштабирование и полёт идут параллельно. DOScale(0.5f) – уменьшает объект до половины размера
        // Ease.InQuad – ускорение в начале, т.е. уменьшается сначала медленно, потом быстрее
        sequence.Join(rectTransform.DOScale(0.5f, 0.6f).SetEase(Ease.InQuad));
        // DOFade(0f, 0.4f) – плавно делает объект полностью прозрачным за 0.4 секунды. SetDelay(0.2f) – начинает затухание не сразу, а через 0.2 секунды после старта всей Sequence
        sequence.Join(canvasGroup.DOFade(0f, 0.4f).SetDelay(0.2f));
        // Когда все анимации завершатся, выполнится этот блок
        sequence.OnComplete(() =>
        {
            onComplete?.Invoke();
            Destroy(gameObject);
        });
        // Запускаем всю цепочку анимаций
        sequence.Play();
    }
}