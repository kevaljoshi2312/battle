using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] Color flashColor = new Color(1f, 0.35f, 0.35f);
    [SerializeField] float flashDuration = 0.12f;

    Renderer targetRenderer;
    Color defaultColor;
    Coroutine flashRoutine;

    void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
    }

    void Start()
    {
        if (targetRenderer != null)
            defaultColor = targetRenderer.material.color;
    }

    public void Play()
    {
        if (targetRenderer == null)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        targetRenderer.material.color = flashColor;
        yield return new WaitForSeconds(flashDuration);

        if (targetRenderer != null)
            targetRenderer.material.color = defaultColor;

        flashRoutine = null;
    }
}
