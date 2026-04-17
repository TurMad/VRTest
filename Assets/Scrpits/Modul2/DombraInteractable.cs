using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable))]
public class DombraInteractable : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private InteractableHighlight highlight;
    [SerializeField] private bool oneShot = true;
    [SerializeField] private InputActionReference[] activateActions;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
    private bool busy;
    private bool played;
    private int hoverCount;

    private void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();

        if (highlight == null)
            highlight = GetComponent<InteractableHighlight>();
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelected);
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);

        for (int i = 0; i < activateActions.Length; i++)
        {
            if (activateActions[i] != null)
                activateActions[i].action.Enable();
        }
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelected);
        interactable.hoverEntered.RemoveListener(OnHoverEntered);
        interactable.hoverExited.RemoveListener(OnHoverExited);

        for (int i = 0; i < activateActions.Length; i++)
        {
            if (activateActions[i] != null)
                activateActions[i].action.Disable();
        }
    }

    private void Update()
    {
        if (hoverCount <= 0)
            return;

        if (WasAnyActivateActionPressedThisFrame())
            TryStartInteraction();
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        TryStartInteraction();
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        hoverCount++;
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        hoverCount = Mathf.Max(0, hoverCount - 1);
    }

    private bool WasAnyActivateActionPressedThisFrame()
    {
        for (int i = 0; i < activateActions.Length; i++)
        {
            if (activateActions[i] != null && activateActions[i].action.WasPressedThisFrame())
                return true;
        }

        return false;
    }

    private void TryStartInteraction()
    {
        if (busy)
            return;

        if (oneShot && played)
            return;

        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        busy = true;
        played = true;

        if (highlight != null)
            highlight.StopHighlight();

        SceneFlowManager.Instance.SetMoveTurnLocked(true);
        SceneFlowManager.Instance.PlayAudio(audioClip);

        yield return SceneFlowManager.Instance.WaitForAudioFinished();

        SceneFlowManager.Instance.SetMoveTurnLocked(false);
        busy = false;
    }
}