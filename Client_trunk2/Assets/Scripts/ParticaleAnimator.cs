using UnityEngine;
using System.Collections;

/// <summary>
/// ParticleSystem不受Time.timeScale影响
/// </summary>
public class ParticaleAnimator : MonoBehaviour
{
    private ParticleSystem[] particles;

    private void Awake()
    {
        particles = GetComponentsInChildren<ParticleSystem>(true);

    }

    // Update is called once per frame
    void Update()
    {
        foreach (var particle in particles)
        {
            particle.Simulate(Time.unscaledDeltaTime, false, false);
        }
    }
}