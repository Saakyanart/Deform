using UnityEngine;
using UnityEngine.UI;
using Deform;
using System.Collections.Generic;

public class RaycastController : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float raycastRange = 100f;
    public KeyCode interactionKey = KeyCode.Mouse0;

    [Header("Crosshair UI")]
    public RawImage crosshairImage;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.red;

    [Header("Long Press Settings")]
    [Tooltip("Время удержания ЛКМ, после которого срабатывает мгновенный взрыв случайного Interaction объекта.")]
    public float longPressDuration = 3f;

    private Camera cam;
    private float holdTime = 0f;
    private bool triggered = false;

    private void Start()
    {
        cam = Camera.main;
        if (crosshairImage != null)
            crosshairImage.color = normalColor;
    }

    private void Update()
    {
        bool pressing = Input.GetKey(interactionKey);

        if (pressing)
        {
            holdTime += Time.deltaTime;

            // если удержание превысило лимит и ещё не сработало
            if (!triggered && holdTime >= longPressDuration)
            {
                triggered = true;
                PopRandomInteraction();
            }
        }
        else
        {
            holdTime = 0f;
            triggered = false;
        }

        HandleRaycast(pressing);
    }

    private void HandleRaycast(bool pressing)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (Physics.Raycast(ray, out RaycastHit hit, raycastRange))
        {
            var target = hit.collider.GetComponent<SpherifyInteraction>() ?? hit.collider.GetComponentInParent<SpherifyInteraction>();

            if (crosshairImage != null)
                crosshairImage.color = target != null ? highlightColor : normalColor;

            if (target != null)
            {
                if (pressing)
                    target.Inflate(Time.deltaTime);
                else
                    target.StopInflation();
            }
        }
        else
        {
            if (crosshairImage != null)
                crosshairImage.color = normalColor;
        }
    }

    private void PopRandomInteraction()
    {
        // Собираем все Interaction объекты
        var all = GameObject.FindGameObjectsWithTag("Interaction");
        var list = new List<SpherifyInteraction>();

        foreach (var obj in all)
        {
            var s = obj.GetComponent<SpherifyInteraction>();
            if (s != null && !s.HasPopped)
                list.Add(s);
        }

        // Если есть доступные объекты — выбираем случайный и "взрываем"
        if (list.Count > 0)
        {
            int index = Random.Range(0, list.Count);
            list[index].ForcePopInstant();
        }
    }
}
