using System.Collections;
using UnityEngine;

public enum ScenarioType
{
    None,
    Normal,
    Leak
}

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject scenarioSelectCanvas;

    [Header("Door")]
    [SerializeField] private Transform entranceDoor;
    [SerializeField] private float closedY = 0f;
    [SerializeField] private float openedY = -130f;
    [SerializeField] private float doorOpenDuration = 1.2f;
    [SerializeField] private AudioSource doorAudioSource;
    
    [SerializeField] private AudioSource matchAudioSource;

    public ScenarioType CurrentScenario { get; private set; } = ScenarioType.None;

    public bool IsNormal => CurrentScenario == ScenarioType.Normal;
    public bool IsLeak => CurrentScenario == ScenarioType.Leak;

    private Coroutine _doorRoutine;
    private bool _doorOpened;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ApplyDoorRotation(closedY);
    }

    public void SelectNormalScenario()
    {
        CurrentScenario = ScenarioType.Normal;
        OnScenarioSelected();
    }

    public void SelectLeakScenario()
    {
        CurrentScenario = ScenarioType.Leak;
        matchAudioSource.enabled = true;
        OnScenarioSelected();
    }

    private void OnScenarioSelected()
    {
        CloseScenarioUI();
        OpenEntranceDoor();
    }

    private void CloseScenarioUI()
    {
        if (scenarioSelectCanvas != null)
            scenarioSelectCanvas.SetActive(false);
    }

    private void OpenEntranceDoor()
    {
        if (_doorOpened || entranceDoor == null)
            return;

        _doorOpened = true;

        if (doorAudioSource != null)
            doorAudioSource.Play();

        if (_doorRoutine != null)
            StopCoroutine(_doorRoutine);

        _doorRoutine = StartCoroutine(OpenDoorRoutine());
    }

    private IEnumerator OpenDoorRoutine()
    {
        float startY = NormalizeAngle(entranceDoor.localEulerAngles.y);
        float time = 0f;

        while (time < doorOpenDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / doorOpenDuration);

            float y = Mathf.Lerp(startY, openedY, t);
            ApplyDoorRotation(y);

            yield return null;
        }

        ApplyDoorRotation(openedY);
        _doorRoutine = null;
    }

    private void ApplyDoorRotation(float yAngle)
    {
        if (entranceDoor == null)
            return;

        Vector3 euler = entranceDoor.localEulerAngles;
        euler.y = yAngle;
        entranceDoor.localEulerAngles = euler;
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}