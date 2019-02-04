using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ListUtilities;

public class WindStream : MonoBehaviour {

    // these lists are used to contain the particles which match
    // the trigger conditions each frame.
    public ParticleSystem ps;
    ParticleSystem.Particle[] particles;
    float radius = 2.0f;
    float k = 50;
    public float speed = 3;
    public float angularSpeed = 160;
    private WindowArray<Vector3> points;

    void Start() {
        points = new WindowArray<Vector3>(50);
    }

    void Update() {
        UpdateInput();
        int N = points.Length;
        if (N == 0 || (points[N - 1] - transform.position).magnitude > .1f) {
            points.Add(transform.position);
        }
        DrawPath();
    }

    void LateUpdate() {
        InitializeParticleSystem();
        int particleCount = ps.GetParticles(particles);

        // Change only the particles that are alive
        for (int i = 0; i < particleCount; i++) {
            // Apply forces to particles when they intersect colliders
            Vector3 q = particles[i].position;
            Vector3 v = particles[i].velocity;
            float dt = Time.deltaTime / 2;
            Vector3 F = CalculateForce(q);
            v += F * dt;
            q += v * dt;

            F = CalculateForce(q);
            v += F * dt;
            particles[i].velocity = v;

            Vector3 d = CalculateVelocity(q);
            particles[i].position += d * Time.deltaTime * speed;
        }

        // Apply the particle changes to the Particle System
        ps.SetParticles(particles, particleCount);
    }

    Vector3 CalculateForce(Vector3 particlePosition) {
        for (int j = points.Length - 1; j > 0; --j) {
            int k0 = j - 1, k1 = j;
            Vector3 p1 = points[k0];
            Vector3 p2 = points[k1];
            if (IsPointInCylinder(particlePosition, p1, p2, radius)) {
                Vector3 a = particlePosition - p1;
                Vector3 b = p2 - p1;
                Vector3 rejection = a - Vector3.Dot(a, b) / b.sqrMagnitude * b;
                // Apply Forces
                Vector3 F1 = -k * (rejection.magnitude - .4f) * rejection.normalized;
                Vector3 F2 = Vector3.Cross(b, rejection);
                return F1 + F2;
            }
        }
        return Vector3.zero;
    }

    Vector3 CalculateVelocity(Vector3 particlePosition) {
        for (int j = points.Length - 1; j > 0; --j) {
            // Check the cylinders in reverse order
            int k0 = j - 1, k1 = j;
            Vector3 p1 = points[k0];
            Vector3 p2 = points[k1];
            if (IsPointInCylinder(particlePosition, p1, p2, radius)) {
                // Only take the first cylinder's velocity
                return (p2 - p1).normalized;
            }
        }
        return Vector3.zero;
    }

    void UpdateInput() {
        Vector2 direction = Vector2.zero;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            direction += Vector2.down;
        } else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            direction += Vector2.up;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
            direction += Vector2.right;
        } else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            direction += Vector2.left;
        }
        direction *= Time.deltaTime * angularSpeed;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x + direction.x, transform.eulerAngles.y + direction.y, 0);
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    bool IsPointInCylinder(Vector3 q, Vector3 p1, Vector3 p2, float r) {
        Vector3 v = p2 - p1;
        Vector3 cross = Vector3.Cross(q - p1, v);
        return Vector3.Dot(q - p1, v) >= 0 && Vector3.Dot(q - p2, v) <= 0 && cross.magnitude / v.magnitude <= r;
    }

    void InitializeParticleSystem() {
        if (ps == null)
            ps = GetComponentInChildren<ParticleSystem>();

        if (particles == null || particles.Length < ps.main.maxParticles)
            particles = new ParticleSystem.Particle[ps.main.maxParticles];
    }

    void DrawPath() {
        for (int i = 0; i < points.Length - 1 ; ++i) {
            Vector3 p1 = points[i], p2 = points[i + 1];
            Debug.DrawLine(p1, p2, Color.cyan);
        }
    }
}
