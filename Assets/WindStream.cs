using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindStream : MonoBehaviour {

    ParticleSystem ps;

    // these lists are used to contain the particles which match
    // the trigger conditions each frame.
    List<ParticleSystem.Particle> inside = new List<ParticleSystem.Particle>();
    ParticleSystem m_System;
    ParticleSystem.Particle[] m_Particles;
    SphereCollider[] colliders;
    float radius = 1.0f;
    float k = 10;

    void Start() {
        ps = GetComponentInChildren<ParticleSystem>();
        colliders = GetComponentsInChildren<SphereCollider>();
    }

    void LateUpdate() {
        InitializeIfNeeded();
        int count = ps.GetParticles(m_Particles);

        // Change only the particles that are alive
        for (int i = 0; i < count; i++) {
            // Apply forces to particles when they intersect colliders
            Vector3 q = m_Particles[i].position;
            Vector3 F = Vector3.zero;
            for (int j = 0; j < colliders.Length - 1; ++j) {
                int k0 = j, k1 = j == colliders.Length - 1 ? 0 : j + 1;
                Vector3 p1 = colliders[k0].transform.position;
                Vector3 p2 = colliders[k1].transform.position;
                if (IsPointInCylinder(q, p1, p2, radius)) {
                    Vector3 a = q - p1;
                    Vector3 b = p2 - p1;
                    Vector3 rejection = a - Vector3.Dot(a, b) / b.sqrMagnitude * b;
                    // Apply Forces
                    Vector3 F1 = b;
                    Vector3 F2 = -k * (rejection.magnitude) * rejection.normalized;
                    F += F1 + F2;
                    Debug.DrawLine(q, q - rejection, Color.red);
                    Debug.DrawLine(q, p1, Color.blue);
                    Debug.DrawLine(p2, p1, Color.yellow);       
                }
            }
            m_Particles[i].velocity += F * Time.deltaTime;
            Debug.DrawLine(q, q + m_Particles[i].velocity, Color.green);
        }

        // Apply the particle changes to the Particle System
        m_System.SetParticles(m_Particles, count);
    }

    bool IsPointInCylinder(Vector3 q, Vector3 p1, Vector3 p2, float r) {
        Vector3 v = p2 - p1;
        Vector3 cross = Vector3.Cross(q - p1, v);
        return Vector3.Dot(q - p1, v) >= 0 && Vector3.Dot(q - p2, v) <= 0 && cross.magnitude / v.magnitude <= r;
    }

    void InitializeIfNeeded() {
        if (m_System == null)
            m_System = GetComponentInChildren<ParticleSystem>();

        if (m_Particles == null || m_Particles.Length < m_System.main.maxParticles)
            m_Particles = new ParticleSystem.Particle[m_System.main.maxParticles];
    }
}
