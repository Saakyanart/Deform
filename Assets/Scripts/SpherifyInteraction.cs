using UnityEngine;
using System.Collections;
using Deform;

[RequireComponent(typeof(Deformable))]
public class SpherifyInteraction : MonoBehaviour
{
    public AudioSource growAudio;
    public AudioClip popClip;
    public float growSpeed = 0.2f;
    public GameObject explosionPrefab;

    private SpherifyDeformer deformer;
    private bool hasPopped = false;
    public bool HasPopped => hasPopped;
    public float Factor => deformer != null ? deformer.Factor : 0f;

    private static bool chainReactionStarted = false;
    private static bool playerTriggeredChain = false;
    private bool passiveInflating = false;

    private void Start()
    {
        deformer = GetComponentInChildren<SpherifyDeformer>();
        if (!deformer)
        {
            Debug.LogError("SpherifyDeformer not found in children.");
            enabled = false;
        }
    }

    public void Inflate(float delta)
    {
        if (hasPopped)
            return;

        if (!growAudio.isPlaying)
            growAudio?.Play();

        deformer.Factor += delta * growSpeed;

        if (deformer.Factor >= 1f)
        {
            deformer.Factor = 1f;
            growAudio?.Stop();
            Pop(true);
        }
    }

    public void StopInflation()
    {
        if (!hasPopped)
            growAudio?.Stop();
    }

    public void ForceGrowToOne()
    {
        if (hasPopped || deformer.Factor >= 1f)
            return;

        StartCoroutine(GrowToOne());
    }

    private IEnumerator GrowToOne()
    {
        while (deformer.Factor < 1f)
        {
            deformer.Factor += Time.deltaTime * growSpeed * 2f;
            yield return null;
        }

        deformer.Factor = 1f;
        Pop(false);
    }

    private void Pop(bool fromPlayer)
    {
        if (hasPopped)
            return;

        hasPopped = true;

        if (popClip != null)
            AudioSource.PlayClipAtPoint(popClip, transform.position);

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 3f);
        }

        gameObject.SetActive(false);

        if (fromPlayer && !chainReactionStarted)
        {
            chainReactionStarted = true;
            playerTriggeredChain = true;
            if (fromPlayer && FinalSequenceManager.Instance != null)
                FinalSequenceManager.Instance.StartFinalSequence();

            TriggerPassiveInflation();
        }
    }

    private void TriggerPassiveInflation()
    {
        var all = GameObject.FindGameObjectsWithTag("Interaction");
        foreach (var obj in all)
        {
            var spherify = obj.GetComponent<SpherifyInteraction>();
            if (spherify != null && !spherify.HasPopped)
                spherify.BeginPassiveInflation();
        }
    }

    public void BeginPassiveInflation()
    {
        if (hasPopped || passiveInflating)
            return;

        StartCoroutine(PassiveInflateRoutine());
    }

    private IEnumerator PassiveInflateRoutine()
    {
        passiveInflating = true;

        float delay = Random.Range(0f, 1.5f);
        yield return new WaitForSeconds(delay);

        while (deformer.Factor < 1f)
        {
            deformer.Factor += Time.deltaTime * growSpeed * 0.1f;
            yield return null;
        }

        deformer.Factor = 1f;
    }
    public void ForcePopInstant()
    {
        if (hasPopped)
            return;

        deformer.Factor = 1f;
        Pop(false);
    }

}