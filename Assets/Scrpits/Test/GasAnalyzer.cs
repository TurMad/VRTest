using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GasAnalyzerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private GameObject normalCanvas;
    [SerializeField] private GameObject leakCanvas;
    [SerializeField] private GameObject leftHandHint;
    [SerializeField] private GameObject rightHandHint;
    [SerializeField] private GameObject scenarioUI;
    [SerializeField] private GameObject finalUI;

    [Header("Controller Roots")]
    [SerializeField] private Transform leftControllerRoot;
    [SerializeField] private Transform rightControllerRoot;

    [Header("Zone Check")]
    [SerializeField] private KitchenZoneVolume kitchenZone;
    [SerializeField] private Transform xrCamera;

    private Transform _currentInteractor;
    private HeldHand _heldHand = HeldHand.None;

    private bool _leftButtonPrev;
    private bool _rightButtonPrev;

    private enum HeldHand
    {
        None,
        Left,
        Right
    }

    private void Reset()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void Awake()
    {
        HideAllResultCanvases();

        if (leftHandHint != null)
            leftHandHint.SetActive(false);

        if (rightHandHint != null)
            rightHandHint.SetActive(false);

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    private void Update()
    {
        if (_heldHand == HeldHand.None)
        {
            HideHints();
            return;
        }

        bool insideKitchen = IsInsideKitchenZone();

        if (!insideKitchen)
        {
            HideHints();
            return;
        }

        UpdateHints();

        switch (_heldHand)
        {
            case HeldHand.Left:
                HandleLeftHandInput();
                break;
            case HeldHand.Right:
                HandleRightHandInput();
                break;
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        _currentInteractor = args.interactorObject.transform;
        _heldHand = DetectHeldHand(_currentInteractor);
        UpdateHints();
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        _currentInteractor = null;
        _heldHand = HeldHand.None;

        HideHints();

        _leftButtonPrev = false;
        _rightButtonPrev = false;
    }

    private HeldHand DetectHeldHand(Transform interactorTransform)
    {
        if (interactorTransform == null)
            return HeldHand.None;

        if (leftControllerRoot != null && interactorTransform.IsChildOf(leftControllerRoot))
            return HeldHand.Left;

        if (rightControllerRoot != null && interactorTransform.IsChildOf(rightControllerRoot))
            return HeldHand.Right;

        string lowerName = interactorTransform.name.ToLower();
        if (lowerName.Contains("left"))
            return HeldHand.Left;
        if (lowerName.Contains("right"))
            return HeldHand.Right;

        return HeldHand.None;
    }

    private bool IsInsideKitchenZone()
    {
        if (kitchenZone == null || xrCamera == null)
            return false;

        return kitchenZone.Contains(xrCamera.position);
    }

    private void UpdateHints()
    {
        bool canShow = IsInsideKitchenZone();

        if (leftHandHint != null)
            leftHandHint.SetActive(canShow && _heldHand == HeldHand.Left);

        if (rightHandHint != null)
            rightHandHint.SetActive(canShow && _heldHand == HeldHand.Right);
    }

    private void HideHints()
    {
        if (leftHandHint != null)
            leftHandHint.SetActive(false);

        if (rightHandHint != null)
            rightHandHint.SetActive(false);
    }

    private void HandleLeftHandInput()
    {
        bool pressed = GetPrimaryButton(XRNode.LeftHand);

        if (pressed && !_leftButtonPrev)
            ToggleCanvas();

        _leftButtonPrev = pressed;
    }

    private void HandleRightHandInput()
    {
        bool pressed = GetPrimaryButton(XRNode.RightHand);

        if (pressed && !_rightButtonPrev)
            ToggleCanvas();

        _rightButtonPrev = pressed;
    }

    private bool GetPrimaryButton(XRNode node)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(node);
        if (!device.isValid)
            return false;

        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressed))
            return pressed;

        return false;
    }

    private void ToggleCanvas()
    {
        if (!IsInsideKitchenZone())
            return;

        GameObject targetCanvas = GetScenarioCanvas();
        if (targetCanvas == null)
            return;

        bool newState = !targetCanvas.activeSelf;

        HideAllResultCanvases();
        targetCanvas.SetActive(newState);
        scenarioUI.gameObject.SetActive(false);
        finalUI.gameObject.SetActive(true);
    }

    private GameObject GetScenarioCanvas()
    {
        if (ScenarioManager.Instance != null && ScenarioManager.Instance.IsLeak)
            return leakCanvas;

        return normalCanvas;
    }

    private void HideAllResultCanvases()
    {
        if (normalCanvas != null)
            normalCanvas.SetActive(false);

        if (leakCanvas != null)
            leakCanvas.SetActive(false);
    }
}