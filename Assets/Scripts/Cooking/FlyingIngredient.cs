using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FlyingIngredient : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private CanvasGroup canvasGroup;
    
    // Принимаем Vector3 вместо Vector2
    public void Setup(Sprite icon, Vector3 startPosition, Vector3 endPosition, System.Action onComplete)
    {
        if (iconImage != null)
            iconImage.sprite = icon;
        
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.position = startPosition;
        
        // Вычисляем среднюю точку с подъёмом вверх
        Vector3 midPoint = (startPosition + endPosition) / 2f + Vector3.up * 150f;
        
        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(rectTransform.DOPath(new Vector3[] { startPosition, midPoint, endPosition }, 0.6f, PathType.CatmullRom)
            .SetEase(Ease.OutQuad));
        
        sequence.Join(rectTransform.DOScale(0.5f, 0.6f).SetEase(Ease.InQuad));
        sequence.Join(canvasGroup.DOFade(0f, 0.4f).SetDelay(0.2f));
        
        sequence.OnComplete(() =>
        {
            onComplete?.Invoke();
            Destroy(gameObject);
        });
        
        sequence.Play();
    }
}