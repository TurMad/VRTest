using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DialogueTriggerZone : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private AudioClip dialogueClip;

    [Header("After Dialogue")]
    [SerializeField] private float fadeToBlackDuration = 0.2f;
    [SerializeField] private float fadeFromBlackDuration = 0.2f;
    [SerializeField] private GameObject[] deactivateAfterDialogue;
    [SerializeField] private XRSimpleInteractable[] interactablesToEnableAfterDialogue;
    [SerializeField] private InteractableHighlight[] highlightsToStartAfterDialogue;
    [SerializeField] private GameObject[] hideTriggerVisualsAfterEnter;

    private bool triggered;

    private void Reset()
    {
        triggerCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        XROrigin foundOrigin = other.GetComponentInParent<XROrigin>();
        if (foundOrigin == null)
            return;

        if (xrOrigin != null && foundOrigin != xrOrigin)
            return;

        triggered = true;
        StartCoroutine(DialogueRoutine());
    }

    private IEnumerator DialogueRoutine()
    {
        if (triggerCollider != null)
            triggerCollider.enabled = false;

        SceneFlowManager.Instance.SetObjectsActive(hideTriggerVisualsAfterEnter, false);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);
        SceneFlowManager.Instance.PlayAudio(dialogueClip);

        yield return SceneFlowManager.Instance.WaitForAudioFinished();

        yield return SceneFlowManager.Instance.FadeToBlack(fadeToBlackDuration);

        SceneFlowManager.Instance.SetObjectsActive(deactivateAfterDialogue, false);

        for (int i = 0; i < interactablesToEnableAfterDialogue.Length; i++)
        {
            if (interactablesToEnableAfterDialogue[i] != null)
                interactablesToEnableAfterDialogue[i].enabled = true;
        }

        for (int i = 0; i < highlightsToStartAfterDialogue.Length; i++)
        {
            if (highlightsToStartAfterDialogue[i] != null)
                highlightsToStartAfterDialogue[i].StartHighlight();
        }

        yield return SceneFlowManager.Instance.FadeFromBlack(fadeFromBlackDuration);

        SceneFlowManager.Instance.SetMoveTurnLocked(false);
    }
}