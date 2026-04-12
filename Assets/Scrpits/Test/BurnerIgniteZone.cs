using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BurnerIgniteZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider igniteTrigger;
    [SerializeField] private GameObject flameVfx;
    [SerializeField] private XRGrabInteractable gasAnalyzerGrab;

    [Header("State")]
    [SerializeField] private bool knobEnabled;
    [SerializeField] private bool burnerIgnited;
    [SerializeField] private bool igniteAttemptProcessed;

    public bool KnobEnabled => knobEnabled;
    public bool BurnerIgnited => burnerIgnited;
    public bool HasLeak => knobEnabled && !burnerIgnited;

    private void Awake()
    {
        if (igniteTrigger == null)
            igniteTrigger = GetComponent<Collider>();

        if (igniteTrigger != null)
            igniteTrigger.enabled = false;

        if (flameVfx != null)
            flameVfx.SetActive(false);

        knobEnabled = false;
        burnerIgnited = false;
        igniteAttemptProcessed = false;
    }

    public void EnableZone()
    {
        knobEnabled = true;
        burnerIgnited = false;
        igniteAttemptProcessed = false;

        if (igniteTrigger != null)
            igniteTrigger.enabled = true;
    }

    public void DisableZone()
    {
        knobEnabled = false;
        burnerIgnited = false;
        igniteAttemptProcessed = false;

        if (igniteTrigger != null)
            igniteTrigger.enabled = false;

        if (flameVfx != null)
            flameVfx.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryIgnite(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryIgnite(other);
    }

    private void TryIgnite(Collider other)
    {
        if (!knobEnabled || igniteAttemptProcessed)
            return;

        MatchIgniteInteractable match = other.GetComponent<MatchIgniteInteractable>();
        if (match == null)
            match = other.GetComponentInParent<MatchIgniteInteractable>();

        if (match == null || !match.IsIgnited)
            return;

        igniteAttemptProcessed = true;

        if (gasAnalyzerGrab != null)
            gasAnalyzerGrab.enabled = true;

        if (ScenarioManager.Instance != null && ScenarioManager.Instance.IsLeak)
        {
            burnerIgnited = false;
            return;
        }
        burnerIgnited = true;

        if (flameVfx != null)
            flameVfx.SetActive(true);
    }
}