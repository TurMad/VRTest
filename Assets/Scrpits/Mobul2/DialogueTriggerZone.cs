using Unity.XR.CoreUtils;
using UnityEngine;

public class DialogueTriggerZone : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private AudioClip dialogueClip;
    [SerializeField] private GameObject[] activateAfterDialogueStart;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        XROrigin foundOrigin = other.GetComponentInParent<XROrigin>();
        if (foundOrigin == null) return;
        if (xrOrigin != null && foundOrigin != xrOrigin) return;

        triggered = true;
        gameObject.SetActive(false);

        SceneFlowManager.Instance.SetXRLocked(true);
        SceneFlowManager.Instance.PlayAudio(dialogueClip);
        SceneFlowManager.Instance.SetObjectsActive(activateAfterDialogueStart, true);
    }
}