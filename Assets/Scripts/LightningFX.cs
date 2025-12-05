using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;

public class LightningFX : MonoBehaviour
{
    [Header("Configuração do Raio")]
    [SerializeField] private Light2D lightningLight;
    [SerializeField] private float minTimeBetweenStrikes = 2f;
    [SerializeField] private float maxTimeBetweenStrikes = 8f;

    [Header("Intensidade")]
    [SerializeField] private float maxIntensity = 3f;
    [SerializeField] private float normalIntensity = 0f;

    [Header("Padrão de Flash")]
    [SerializeField] private int minFlashes = 1;
    [SerializeField] private int maxFlashes = 3;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float delayBetweenFlashes = 0.05f;

    public UnityEvent LightningSound;
    void Start()
    {
        if (lightningLight == null)
            lightningLight = GetComponent<Light2D>();

        lightningLight.intensity = normalIntensity;
        StartCoroutine(LightningLoop());
    }

    IEnumerator LightningLoop()
    {
        while (true)
        {
            // Esperar tempo aleatório
            float waitTime = Random.Range(minTimeBetweenStrikes, maxTimeBetweenStrikes);
            yield return new WaitForSeconds(waitTime);

            // Fazer o raio
            yield return StartCoroutine(Strike());
        }
    }

    IEnumerator Strike()
    {
        int flashes = Random.Range(minFlashes, maxFlashes + 1);

        for (int i = 0; i < flashes; i++)
        {
            LightningSound?.Invoke();

            // Flash on
            lightningLight.intensity = Random.Range(maxIntensity * 0.7f, maxIntensity);
            yield return new WaitForSeconds(flashDuration);

            // Flash off
            lightningLight.intensity = normalIntensity;

            if (i < flashes - 1)
                yield return new WaitForSeconds(delayBetweenFlashes);
        }
    }

    // Método público para forçar um raio
    [ContextMenu("Forçar Raio")]
    public void ForceLightning()
    {
        StartCoroutine(Strike());
    }
}