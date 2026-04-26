using UnityEngine;
public interface IInteractable
{
    /// <summary>
    /// то, что будет происходить при взаимодействии с объектом. Например, открытие двери, поднятие предмета и т.д.
    /// </summary>
    void Interact();

    /// <summary>
    /// текст, который будет отображаться в UI при наведении на объект.
    /// Например, "Нажмите E, чтобы открыть дверь" или "Нажмите E, чтобы открыть дверь
    /// </summary>
    /// <returns></returns>
    string GetInteractText();

    Outline GetOutline();

    Transform GetTransform();
}