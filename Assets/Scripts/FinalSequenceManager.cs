using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System.Collections;

public class FinalSequenceManager : MonoBehaviour
{
    [Header("Ссылки")]
    public Camera playerCamera;
    public GameObject playerController; // компонент, отвечающий за управление
    public Volume globalVolume;
    public GameObject finalUIPanel;

    [Header("Параметры эффекта")]
    [Tooltip("Время, за которое интенсивность виньетки плавно возрастает.")]
    public float vignetteDuration = 10f;

    [Tooltip("Целевая интенсивность виньетки в конце финала.")]
    public float vignetteTarget = 0.35f;

    [Tooltip("Скорость увеличения коллайдеров Interaction объектов.")]
    public float colliderGrowthSpeed = 0.3f;

    [Tooltip("Интенсивность тряски камеры.")]
    public float shakeIntensity = 0.15f;

    [Tooltip("Длительность тряски камеры.")]
    public float shakeDuration = 5f;

    private bool isFinalStarted = false;
    private Vignette vignette;

    private static FinalSequenceManager instance;

    public static FinalSequenceManager Instance => instance;

    private void Awake()
    {
        instance = this;
        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out vignette);
            if (vignette != null)
                vignette.intensity.value = 0f;
        }

        if (finalUIPanel != null)
            finalUIPanel.SetActive(false);
    }

    public void StartFinalSequence()
    {
        if (isFinalStarted) return;
        isFinalStarted = true;
        StartCoroutine(FinalRoutine());
    }

    private IEnumerator FinalRoutine()
    {
        // 1. Отключаем управление
        if (playerController != null)
        {
            var comp = playerController.GetComponent<MonoBehaviour>();
            if (comp != null)
                comp.enabled = false;
        }

        // 2. Запускаем тряску камеры
        StartCoroutine(CameraShake());

        // 3. Увеличиваем коллайдеры объектов
        StartCoroutine(ExpandColliders());

        // 4. Постепенно усиливаем Vignette
        if (vignette != null)
        {
            float t = 0;
            while (t < vignetteDuration)
            {
                t += Time.deltaTime;
                vignette.intensity.value = Mathf.Lerp(0f, vignetteTarget, t / vignetteDuration);
                yield return null;
            }
        }

        // 5. Останавливаем игру и показываем финальную панель
        Time.timeScale = 0f;
        if (finalUIPanel != null)
            finalUIPanel.SetActive(true);
    }

    private IEnumerator CameraShake()
    {
        if (playerCamera == null) yield break;

        Vector3 originalPos = playerCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;
            playerCamera.transform.localPosition = originalPos + new Vector3(x, y, 0);
            yield return null;
        }

        playerCamera.transform.localPosition = originalPos;
    }

    private IEnumerator ExpandColliders()
    {
        var all = GameObject.FindGameObjectsWithTag("Interaction");

        while (true)
        {
            foreach (var obj in all)
            {
                var coll = obj.GetComponent<Collider>();
                if (coll != null)
                {
                    coll.transform.localScale += Vector3.one * (colliderGrowthSpeed * Time.deltaTime);
                }
            }

            yield return null;
        }
    }

    // Вызывается кнопками UI
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
