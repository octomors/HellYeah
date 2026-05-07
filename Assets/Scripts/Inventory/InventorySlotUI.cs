using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

// Один слот в окне инвентаря
public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI элементы")]
    public Image iconImage;
    public TextMeshProUGUI amountText;
    public Image background;
    public Image border;

    [Header("Цвета")]
    public Color normalBorderColor = new Color32(0x5A, 0x30, 0x10, 0xFF); //цвет рамки в обычном состоянии
    public Color hoverBorderColor = new Color32(0xE8, 0xC5, 0x6A, 0xFF); //цвет рамки при наведении мыши
    public Color emptyColor = new Color32(0x1E, 0x0D, 0x04, 0x60); //цвет фона, когда слот пуст
    public Color filledColor = new Color32(0x2A, 0x13, 0x06, 0xFF); //цвет фона, когда в слоте есть ингредиент

    // Колбэки - назначаются из InventoryUI
    public Action<Ingredient, int> OnHovered;
    public Action OnUnhovered;

    private Ingredient _ingredient;
    private int _amount;

    // Метод вызывается, когда инвентарь обновляется, и передаёт слоту, что в нём должно отображаться
    public void Setup(Ingredient ingredient, int amount)
    {
        _ingredient = ingredient;
        _amount = amount;

        if (ingredient == null) { SetEmpty(); return; }

        if (iconImage != null)
        {
            iconImage.sprite  = ingredient.icon;
            iconImage.enabled = ingredient.icon != null;
        }
        if (amountText != null)
            amountText.text = $"×{amount}";
        if (background != null)
            background.color = filledColor;
        if (border != null)
            border.color = normalBorderColor;
    }

    // Делает слот пустым
    public void SetEmpty()
    {
        _ingredient = null;
        _amount     = 0;

        if (iconImage != null)  { iconImage.sprite = null; iconImage.enabled = false; }
        if (amountText != null)   amountText.text = "";
        if (background != null)   background.color = emptyColor;
        if (border != null)       border.color     = normalBorderColor;
    }

    // Автоматически вызывается, когда курсор мыши заходит в область элемента
    public void OnPointerEnter(PointerEventData e)
    {
        if (border != null) border.color = hoverBorderColor;

        // Показываем детали только если слот не пустой
        if (_ingredient != null)
            OnHovered?.Invoke(_ingredient, _amount);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (border != null) border.color = normalBorderColor;
        OnUnhovered?.Invoke();
    }
}