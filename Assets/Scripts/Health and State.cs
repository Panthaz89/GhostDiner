using UnityEngine;
using UnityEngine.Events;

public class NpcVitals : MonoBehaviour
{
    public enum State
    {
        Satisfied,
        Happy,
        Annoyed,
        Agitated,
        Furious
    }

    [Header("Health Settings")]
    [Tooltip("Maximum health of this NPC.")]
    public float maxHealth = 100f;

    [Tooltip("Current health value.")]
    public float health = 100f;

    [Header("Happiness (0–100)")]
    [Range(0, 100)] public int happiness = 100;

    [Header("State")]
    [SerializeField] private State currentState = State.Satisfied;
    public UnityEvent<State> onStateChanged;

    [Header("Renderers (auto-finds if empty)")]
    public Renderer[] targetRenderers;

    [Header("Color by Health %")]
    public Gradient healthGradient = DefaultGradient();
    public bool tintEmission = false;

    private MaterialPropertyBlock _mpb;
    private int _baseColorID, _colorID, _emissionColorID;
    private State _lastState;

    // --- Computed property ---
    public float HealthPercent => Mathf.Clamp01(health / Mathf.Max(1f, maxHealth));

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        _baseColorID = Shader.PropertyToID("_BaseColor");
        _colorID = Shader.PropertyToID("_Color");
        _emissionColorID = Shader.PropertyToID("_EmissionColor");

        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            var r = GetComponentInChildren<Renderer>();
            if (r != null) targetRenderers = new[] { r };
        }

        health = Mathf.Clamp(health, 0, maxHealth);
        happiness = Mathf.Clamp(happiness, 0, 100);

        UpdateState(forceEvent: true);
        UpdateColor();
    }

    private void OnValidate()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        happiness = Mathf.Clamp(happiness, 0, 100);
        UpdateState();
        UpdateColor();
    }

    // --- Public API ---

    public void ApplyDamage(float amount)
    {
        if (amount <= 0f) return;
        health = Mathf.Max(0f, health - amount);
        UpdateColor();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        health = Mathf.Min(maxHealth, health + amount);
        UpdateColor();
    }

    public void SetHealth(float value)
    {
        health = Mathf.Clamp(value, 0f, maxHealth);
        UpdateColor();
    }

    public void AddHappiness(int amount)
    {
        if (amount == 0) return;
        happiness = Mathf.Clamp(happiness + amount, 0, 100);
        UpdateState();
    }

    public void SetHappiness(int value)
    {
        happiness = Mathf.Clamp(value, 0, 100);
        UpdateState();
    }

    public State GetState() => currentState;

    // --- Internals ---

    private void UpdateState(bool forceEvent = false)
    {
        var newState = EvaluateStateFromHappiness(happiness);
        currentState = newState;

        if (forceEvent || newState != _lastState)
        {
            _lastState = newState;
            onStateChanged?.Invoke(newState);
        }
    }

    private static State EvaluateStateFromHappiness(int h)
    {
        if (h <= 0) return State.Furious;
        if (h <= 25) return State.Agitated;
        if (h <= 50) return State.Annoyed;
        if (h <= 75) return State.Happy;
        return State.Satisfied;
    }

    private void UpdateColor()
    {
        if (targetRenderers == null) return;

        Color c = healthGradient.Evaluate(HealthPercent);

        foreach (var r in targetRenderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(_mpb);

            if (HasProperty(r, _baseColorID))
                _mpb.SetColor(_baseColorID, c);
            else if (HasProperty(r, _colorID))
                _mpb.SetColor(_colorID, c);

            if (tintEmission && HasProperty(r, _emissionColorID))
                _mpb.SetColor(_emissionColorID, c * 0.5f);

            r.SetPropertyBlock(_mpb);
        }
    }

    private static bool HasProperty(Renderer r, int propId)
    {
        var mat = r.sharedMaterial;
        return mat != null && mat.HasProperty(propId);
    }

    private static Gradient DefaultGradient()
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.red, 0f),
                new GradientColorKey(Color.yellow, 0.5f),
                new GradientColorKey(Color.green, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );
        return g;
    }
}
