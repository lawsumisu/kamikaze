using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ListUtilities;

public class WindStream : MonoBehaviour {

    class WindStreamForceFieldInfo {
        public static WindStreamForceFieldInfo ZERO = new WindStreamForceFieldInfo(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
        private Vector3 _projection, _rejection, _axis, _force;

        public WindStreamForceFieldInfo(Vector3 projection, Vector3 rejection, Vector3 axis, Vector3 force) {
            this._projection = projection;
            this._rejection = rejection;
            this._force = force;
            this._axis = axis;
        }

        public Vector3 projection {
            get { return _projection; }
        }

        public Vector3 rejection {
            get { return _rejection; }
        }

        public Vector3 force {
            get { return _force; }
        }

        public Vector3 axis {
            get { return _axis; }
        }
    }

    // these lists are used to contain the particles which match
    // the trigger conditions each frame.
    public ParticleSystem ps;
    ParticleSystem.Particle[] particles;
    float minRadius = .5f;
    float maxRadius = 3.0f;
    float k = 30;
    public float speed = 3;
    public float angularSpeed = 200;
    private WindowArray<Vector3> points;
    private bool isDebug = false;
    private float startSpeed;

    void Start() {
        points = new WindowArray<Vector3>(50);
        startSpeed = speed;
    }

    void Update() {
        UpdateInput();
        int N = points.Length;
        if (N == 0 || (points[N - 1] - transform.position).magnitude > .1f) {
            points.Add(transform.position);
        }
        if (isDebug) DrawPath();
    }

    void LateUpdate() {
        InitializeParticleSystem();
        int particleCount = ps.GetParticles(particles);

        // Change only the particles that are alive
        for (int i = 0; i < particleCount; i++) {
            // Apply forces to particles when they intersect colliders

            // Calculate velocity change with RK2 in order to have a higher-order bound on the error caused by the spring forces
            Vector3 q = particles[i].position;
            Vector3 v = particles[i].velocity;
            float dt = Time.deltaTime / 2;
            Vector3 F = CalculateForceFieldInfo(q).force;
            v += F * dt;
            q += v * dt;

            WindStreamForceFieldInfo FFI = CalculateForceFieldInfo(q);
            v += FFI.force * dt;
            particles[i].velocity = v;

            Vector3 rotatedRejection = Quaternion.AngleAxis(10, FFI.projection) * FFI.rejection;
            Vector3 newPosition = particles[i].position - FFI.rejection + rotatedRejection;
            particles[i].position = newPosition + FFI.axis;
            particles[i].remainingLifetime += Time.deltaTime;
        }

        // Apply the particle changes to the Particle System
        ps.SetParticles(particles, particleCount);
    }

    WindStreamForceFieldInfo CalculateForceFieldInfo(Vector3 particlePosition) {
        float radialMultiplier = speed / startSpeed;
        for (int j = points.Length - 1; j > 0; --j) {
            int k0 = j - 1, k1 = j;
            Vector3 p1 = points[k0];
            Vector3 p2 = points[k1];
            float r1 = Mathf.Lerp(minRadius * radialMultiplier, maxRadius * radialMultiplier, (points.Length - k1 - 1 ) / (points.Length - 1.0f));
            float r2 = Mathf.Lerp(minRadius * radialMultiplier, maxRadius * radialMultiplier, (points.Length - k0 - 1) / (points.Length - 1.0f));
            if (IsPointInCone(particlePosition, p1, p2, r1, r2)) {
                Vector3 a = particlePosition - p1;
                Vector3 b = p2 - p1;
                Vector3[] projectionData = GetProjection(a, b);
                Vector3 rejection = projectionData[1];
                Vector3 force = -k * (rejection.magnitude * .4f) * rejection.normalized;
                return new WindStreamForceFieldInfo(projectionData[0], rejection, b, force);
            }
        }
        return WindStreamForceFieldInfo.ZERO;
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

        if (Input.GetKeyDown(KeyCode.Q)) {
            isDebug = !isDebug;
        }
        if (Input.GetKey(KeyCode.Space)) {
            speed += .05f;
        } else {
            speed = Mathf.Max(speed - .1f, startSpeed);
        }
    }

    bool IsPointInCylinder(Vector3 q, Vector3 p1, Vector3 p2, float r) {
        return IsPointInCone(q, p1, p2, r, r);
    }

    bool IsPointInCone(Vector3 q, Vector3 p1, Vector3 p2, float r1, float r2) {
        Vector3 axis = p2 - p1;
        if (Vector3.Dot(q - p1, axis) >= 0 && Vector3.Dot(q - p2, axis) <= 0) {
            Vector3[] projectionData = GetProjection(q - p1, axis);
            Vector3 projection = projectionData[0];
            float t = projection.magnitude / axis.magnitude;
            float r = Mathf.Lerp(r1, r2, t);
            Vector3 rejection = projectionData[1];
            return rejection.magnitude <= r;
        }
        else return false;
    }

    void InitializeParticleSystem() {
        if (ps == null)
            ps = GetComponentInChildren<ParticleSystem>();

        if (particles == null || particles.Length < ps.main.maxParticles)
            particles = new ParticleSystem.Particle[ps.main.maxParticles];
    }

    void DrawPath() {
        int N = points.Length;
        float radialMultiplier = speed / startSpeed;
        for (int i = 0; i < N - 1 ; ++i) {
            Vector3 p1 = points[i], p2 = points[i + 1];
            Vector3 center = (p1 + p2) / 2;
            Vector3 axis = p2 - p1;

            float r = Mathf.Lerp(minRadius * radialMultiplier, maxRadius * radialMultiplier, (N - 1 - (i + .5f)) / (N - 1.0f));
            DrawCircle(center, axis, r, Color.yellow);
            Debug.DrawLine(p1, p2, Color.cyan);
        }
    }
    void DrawCircle(Vector3 center, Vector3 axis, float radius, Color color) {
        Vector3 normal = axis.normalized;
        Vector3 orthoAxis = new Vector3(normal.z, 0, -normal.x).normalized;
        int N = 10;
        for (int i = 0; i < N; ++i) {
            float theta1 = (i * 360.0f) / N;
            float theta2 = ((i + 1) * 360.0f) / N;
            Vector3 p1 = center + Quaternion.AngleAxis(theta1, axis) * orthoAxis * radius;
            Vector3 p2 = center + Quaternion.AngleAxis(theta2, axis) * orthoAxis * radius;
            Debug.DrawLine(p1, p2, color);
        }
    }

    Vector3[] GetProjection(Vector3 a, Vector3 b) {
        Vector3 projection = Vector3.Dot(a, b) / b.sqrMagnitude * b;
        Vector3 rejection = a - projection;
        return new Vector3[] { projection, rejection };
    }
}
