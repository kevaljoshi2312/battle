using System.Collections;
using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    [SerializeField] float fallDuration = 0.35f;
    [SerializeField] float holdDuration = 0.15f;
    [SerializeField] float shrinkDuration = 0.25f;

    bool isPlaying;

    public void FinishImmediately()
    {
        if (!isPlaying)
            return;

        StopAllCoroutines();
        Destroy(gameObject);
    }

    public void Play()
    {
        if (isPlaying)
            return;

        isPlaying = true;
        DisableUnit();
        StartCoroutine(PlayRoutine());
    }

    void DisableUnit()
    {
        foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
        {
            if (behaviour != this)
                behaviour.enabled = false;
        }

        foreach (Collider collider in GetComponents<Collider>())
            collider.enabled = false;

        SelectionRing ring = GetComponent<SelectionRing>();
        ring?.Hide();

        HealthBar healthBar = GetComponent<HealthBar>();
        healthBar?.Hide();
    }

    IEnumerator PlayRoutine()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(90f, Random.Range(-20f, 20f), 0f);

        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        yield return new WaitForSeconds(holdDuration);

        Vector3 startScale = transform.localScale;
        elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkDuration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
