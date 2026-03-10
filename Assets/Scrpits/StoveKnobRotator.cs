using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(XRSimpleInteractable))]
public class StoveKnobRotator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform rotatingPart;
    [SerializeField] private Transform referenceSpace;

    [Header("Angles")]
    [SerializeField] private float offAngle = 0f;
    [SerializeField] private float onAngle = -90f;

    [Header("Input")]
    [SerializeField] private bool useXAxis = true;
    [SerializeField] private bool invertDirection = false;
    [SerializeField] private float switchThreshold = 0.015f;

    [Header("State")]
    [SerializeField] private bool startOn = false;

    [Header("Events")]
    public UnityEvent onTurnedOn;
    public UnityEvent onTurnedOff;

    private XRSimpleInteractable _interactable;
    private Transform _currentInteractor;
    private float _lastAxisValue;
    private bool _isOn;

    public bool IsOn => _isOn;

    private void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();

        _interactable.selectEntered.AddListener(OnSelectEntered);
        _interactable.selectExited.AddListener(OnSelectExited);

        if (rotatingPart == null)
            rotatingPart = transform;

        _isOn = startOn;
        ApplyStateInstant();
    }

    private void OnDestroy()
    {
        if (_interactable == null)
            return;

        _interactable.selectEntered.RemoveListener(OnSelectEntered);
        _interactable.selectExited.RemoveListener(OnSelectExited);
    }

    private void Update()
    {
        if (_currentInteractor == null)
            return;

        float currentAxis = GetInteractorAxisPosition(_currentInteractor);
        float delta = currentAxis - _lastAxisValue;

        if (invertDirection)
            delta = -delta;

        if (!_isOn && delta <= -switchThreshold)
        {
            SetState(true);
            _lastAxisValue = currentAxis;
        }
        else if (_isOn && delta >= switchThreshold)
        {
            SetState(false);
            _lastAxisValue = currentAxis;
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        _currentInteractor = args.interactorObject.transform;
        _lastAxisValue = GetInteractorAxisPosition(_currentInteractor);
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        _currentInteractor = null;
    }

    private float GetInteractorAxisPosition(Transform interactorTransform)
    {
        if (referenceSpace != null)
        {
            Vector3 localPos = referenceSpace.InverseTransformPoint(interactorTransform.position);
            return useXAxis ? localPos.x : localPos.y;
        }

        Vector3 worldPos = interactorTransform.position;
        return useXAxis ? worldPos.x : worldPos.y;
    }

    private void SetState(bool newState)
    {
        if (_isOn == newState)
            return;

        _isOn = newState;
        ApplyStateInstant();

        if (_isOn)
            onTurnedOn?.Invoke();
        else
            onTurnedOff?.Invoke();
    }

    private void ApplyStateInstant()
    {
        float targetAngle = _isOn ? onAngle : offAngle;

        Vector3 localEuler = rotatingPart.localEulerAngles;
        localEuler.z = targetAngle;
        rotatingPart.localEulerAngles = localEuler;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (rotatingPart == null)
            return;

        float targetAngle = startOn ? onAngle : offAngle;
        Vector3 localEuler = rotatingPart.localEulerAngles;
        localEuler.z = targetAngle;
        rotatingPart.localEulerAngles = localEuler;
    }
#endif
}