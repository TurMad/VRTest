using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Module1InteractionManager : MonoBehaviour
{
    [Header("Items")]
    [SerializeField] private Module1GrabItem[] allItems;

    [Header("Transition")]
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Transform nextViewPoint;
    [SerializeField] private float fadeToBlackDuration = 0.25f;
    [SerializeField] private float fadeFromBlackDuration = 0.25f;
    [SerializeField] private bool unlockAfterTransition = false;

    private int interactedCount;
    private bool specialAudioPlaying;
    private bool transitionStarted;
    private bool pendingTransition;

    public void NotifyItemInteracted(Module1GrabItem item)
    {
        interactedCount++;

        if (interactedCount < allItems.Length)
            return;

        if (transitionStarted)
            return;

        if (specialAudioPlaying)
            pendingTransition = true;
        else
            StartCoroutine(TransitionRoutine());
    }

    public void PlaySpecialItemAudio(Module1GrabItem sourceItem, AudioClip clip)
    {
        if (specialAudioPlaying || transitionStarted || clip == null)
            return;

        StartCoroutine(SpecialAudioRoutine(sourceItem, clip));
    }

    private IEnumerator SpecialAudioRoutine(Module1GrabItem sourceItem, AudioClip clip)
    {
        specialAudioPlaying = true;

        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        SetAllGrabInteractablesEnabled(false, sourceItem);

        SceneFlowManager.Instance.PlayAudio(clip);
        yield return SceneFlowManager.Instance.WaitForAudioFinished();

        SetAllGrabInteractablesEnabled(true, null);

        SceneFlowManager.Instance.SetMoveTurnLocked(false);

        specialAudioPlaying = false;

        if (pendingTransition && !transitionStarted)
            StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        transitionStarted = true;
        pendingTransition = false;

        SceneFlowManager.Instance.SetMoveTurnLocked(true);
        SceneFlowManager.Instance.SetXRLocked(true);
        SetAllGrabInteractablesEnabled(false, null);

        yield return SceneFlowManager.Instance.FadeToBlack(fadeToBlackDuration);

        MoveXROriginToPoint();

        yield return SceneFlowManager.Instance.FadeFromBlack(fadeFromBlackDuration);

        if (unlockAfterTransition)
        {
            SetAllGrabInteractablesEnabled(true, null);
            SceneFlowManager.Instance.SetXRLocked(false);
            SceneFlowManager.Instance.SetMoveTurnLocked(false);
        }
    }

    private void SetAllGrabInteractablesEnabled(bool value, Module1GrabItem exceptItem)
    {
        for (int i = 0; i < allItems.Length; i++)
        {
            if (allItems[i] == null || allItems[i].GrabInteractable == null)
                continue;

            if (exceptItem != null && allItems[i] == exceptItem)
                continue;

            allItems[i].GrabInteractable.enabled = value;
        }
    }

    private void MoveXROriginToPoint()
    {
        if (xrOrigin == null || nextViewPoint == null)
            return;

        Transform cameraTransform = xrOrigin.Camera.transform;

        Vector3 cameraOffset = xrOrigin.transform.position - cameraTransform.position;
        cameraOffset.y = 0f;

        xrOrigin.transform.position = nextViewPoint.position + cameraOffset;

        Vector3 forward = nextViewPoint.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude > 0.001f)
            xrOrigin.transform.rotation = Quaternion.LookRotation(forward);
    }
}