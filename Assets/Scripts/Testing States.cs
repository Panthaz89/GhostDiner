using UnityEngine;
using UnityEngine.UI;      // for UI Text
using TMPro;               // for TMP support

public class TestVitalsDrain : MonoBehaviour
{
    [Header("Vitals Reference")]
    public NpcVitals vitals;

    [Header("Drain Settings")]
    public float healthDrainPerSecond = 2f;
    public int happinessDrainPerSecond = 2;

    [Header("Display (optional)")]
    [Tooltip("World-space or UI text to display happiness state.")]
    public Text uiText;
    public TextMesh worldText;
    public TMP_Text tmpText;
    public string prefix = "Happiness: ";

    private NpcVitals.State lastState;

    void Awake()
    {
        if (vitals == null)
            vitals = GetComponent<NpcVitals>();

        if (vitals == null)
        {
            Debug.LogError("TestVitalsDrain needs an NpcVitals reference!");
            enabled = false;
            return;
        }

        // Initialize text
        UpdateText(vitals.GetState());
        lastState = vitals.GetState();
    }

    void Update()
    {
        if (vitals == null) return;

        // --- Drain logic ---
        vitals.ApplyDamage(healthDrainPerSecond * Time.deltaTime);

        int drain = Mathf.RoundToInt(happinessDrainPerSecond * Time.deltaTime);
        if (drain > 0)
            vitals.AddHappiness(-drain);

        // --- Text update ---
        var state = vitals.GetState();
        if (state != lastState)
        {
            UpdateText(state);
            lastState = state;
        }
    }

    private void UpdateText(NpcVitals.State state)
    {
        string stateText = state switch
        {
            NpcVitals.State.Satisfied => "Satisfied",
            NpcVitals.State.Happy => "Happy",
            NpcVitals.State.Annoyed => "Annoyed",
            NpcVitals.State.Agitated => "Agitated",
            NpcVitals.State.Furious => "Furious",
            _ => state.ToString()
        };

        string finalText = string.IsNullOrEmpty(prefix) ? stateText : $"{prefix}{stateText}";

        if (uiText != null)
            uiText.text = finalText;
        if (worldText != null)
            worldText.text = finalText;
        if (tmpText != null)
            tmpText.text = finalText;
    }
}
