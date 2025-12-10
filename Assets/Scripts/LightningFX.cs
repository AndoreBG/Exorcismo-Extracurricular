using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;

public class LightningFX : MonoBehaviour
{
    [Header("Configuração do Raio")]
    [SerializeField] private List<Light2D> lightningLights = new List<Light2D>();
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
        // Se a lista estiver vazia, tenta pegar o Light2D do próprio objeto
        if (lightningLights.Count == 0)
        {
            Light2D localLight = GetComponent<Light2D>();
            if (localLight != null)
                lightningLights.Add(localLight);
        }

        // Inicializa todas as luzes com intensidade normal
        SetAllLightsIntensity(normalIntensity);

        StartCoroutine(LightningLoop());
    }

    IEnumerator LightningLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minTimeBetweenStrikes, maxTimeBetweenStrikes);
            yield return new WaitForSeconds(waitTime);

            yield return StartCoroutine(Strike());
        }
    }

    IEnumerator Strike()
    {
        int flashes = Random.Range(minFlashes, maxFlashes + 1);

        for (int i = 0; i < flashes; i++)
        {
            LightningSound?.Invoke();

            // Flash on - todas as luzes
            float intensity = Random.Range(maxIntensity * 0.7f, maxIntensity);
            SetAllLightsIntensity(intensity);
            yield return new WaitForSeconds(flashDuration);

            // Flash off - todas as luzes
            SetAllLightsIntensity(normalIntensity);

            if (i < flashes - 1)
                yield return new WaitForSeconds(delayBetweenFlashes);
        }
    }

    /// <summary>
    /// Define a intensidade de todas as luzes da lista
    /// </summary>
    private void SetAllLightsIntensity(float intensity)
    {
        foreach (Light2D light in lightningLights)
        {
            if (light != null)
                light.intensity = intensity;
        }
    }

    /// <summary>
    /// Adiciona uma luz à lista em runtime
    /// </summary>
    public void AddLight(Light2D light)
    {
        if (light != null && !lightningLights.Contains(light))
            lightningLights.Add(light);
    }

    /// <summary>
    /// Remove uma luz da lista em runtime
    /// </summary>
    public void RemoveLight(Light2D light)
    {
        lightningLights.Remove(light);
    }

    [ContextMenu("Forçar Raio")]
    public void ForceLightning()
    {
        StartCoroutine(Strike());
    }
}