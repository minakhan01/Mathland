using UnityEngine;
using System.Collections;

/// <summary>
/// Attach this script on a particle system.
/// All its and its children's particles will be affected by the target force field.
/// </summary>
public class ForceOnParticle : MonoBehaviour
{
    [FFToolTip("The force field that will be used.")]
    public ForceField field;

    [FFToolTip("Modifier of the force.")]
    public float factor = 1;

    ParticleEmitter[] pes;
    ParticleSystem[] pss;

    // Use this for initialization
    void Start()
    {
        pes = GetComponentsInChildren<ParticleEmitter>(true);
        pss = GetComponentsInChildren<ParticleSystem>(true);
    }

    void Update()
    {
        if (field != null)
        {
            foreach (ParticleEmitter pe in pes)
            {
                Transform t = pe.transform;
                Particle[] particles = pe.particles;
                int num = particles.Length;
                for (int i = 0; i < num; i++)
                {
                    Vector3 force;
                    if (!pe.useWorldSpace)
                    {
                        force = t.InverseTransformDirection(field.GetForce(t.TransformPoint(particles[i].position)));
                    }
                    else
                    {
                        force = field.GetForce(particles[i].position);
                    }
                    particles[i].velocity += force * factor;
                }
                pe.particles = particles;
            }
            foreach (ParticleSystem ps in pss)
            {
                Transform t = ps.transform;
                ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.maxParticles];
                int num = ps.GetParticles(particles);
                for (int i = 0; i < num; i++)
                {
                    Vector3 force;
                    if (ps.simulationSpace == ParticleSystemSimulationSpace.Local)
                    {
                        force = t.InverseTransformDirection(field.GetForce(t.TransformPoint(particles[i].position)));
                    }
                    else
                    {
                        force = field.GetForce(particles[i].position);
                    }
                    particles[i].velocity += force * factor;
                }
                ps.SetParticles(particles, num);
            }
        }
    }

}
