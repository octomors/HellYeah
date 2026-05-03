using System;
using System.Collections;
using UnityEngine;

// Управляет переходом между камерой от первого лица игрока и фиксированной камерой готовки.
public class CookingCameraController : MonoBehaviour
{
    [Header("Камера готовки")]
    public Camera cookingCamera;

    [Header("Затемнение экрана")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 0.35f;
    public string firstPersonCameraName = "First Person Camera";
    public string playerRootName = "Player(Clone)";

    private Camera _firstPersonCamera;
    private bool _isInCookingMode = false;
    private GameObject _playerRoot;

    private void Awake()
    {
        // Cooking-камера выключена по умолчанию
        if (cookingCamera != null)
            cookingCamera.gameObject.SetActive(false);

        if (fadePanel != null)
            fadePanel.alpha = 0f;
    }

    private void FindPlayerComponents()
    {
        if (_firstPersonCamera == null)
        {
            GameObject camObj = GameObject.Find(firstPersonCameraName);
            if (camObj != null)
                _firstPersonCamera = camObj.GetComponent<Camera>();
            else
                Debug.LogWarning($"[CookingCamera] Не найден объект '{firstPersonCameraName}'");
        }

        if (_playerRoot == null)
        {
            _playerRoot = GameObject.Find(playerRootName);
            if (_playerRoot == null)
                Debug.LogWarning($"[CookingCamera] Не найден объект '{playerRootName}'");
        }
    }

    public void EnterCookingMode(Action onReady)
    {
        if (_isInCookingMode) return;
        _isInCookingMode = true;
        FindPlayerComponents();
        StartCoroutine(TransitionCoroutine(true, onReady));
    }

    public void ExitCookingMode(Action onComplete = null)
    {
        if (!_isInCookingMode) return;
        StartCoroutine(TransitionCoroutine(false, () =>
        {
            _isInCookingMode = false;
            onComplete?.Invoke();
        }));
    }

    private IEnumerator TransitionCoroutine(bool enteringCooking, Action onDone)
    {
        // Затемнение
        yield return StartCoroutine(Fade(0f, 1f));

        // Переключение
        if (enteringCooking)
        {
            if (_firstPersonCamera != null) _firstPersonCamera.gameObject.SetActive(false); // отключить камеру от первого лица
            if (_playerRoot != null) _playerRoot.SetActive(false); // скрыть игрока
            if (cookingCamera != null) cookingCamera.gameObject.SetActive(true); // включить камеру для готовки
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (cookingCamera != null) cookingCamera.gameObject.SetActive(false);
            if (_playerRoot != null) _playerRoot.SetActive(true);
            if (_firstPersonCamera != null) _firstPersonCamera.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        onDone?.Invoke();

        yield return StartCoroutine(Fade(1f, 0f));
    }
    
    // Плавное переключение цвета (затемнение/высветление)
    private IEnumerator Fade(float from, float to)
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;
        fadePanel.alpha = from;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        fadePanel.alpha = to;
    }
}