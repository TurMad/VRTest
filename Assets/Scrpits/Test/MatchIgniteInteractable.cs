using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MatchIgniteInteractable : MonoBehaviour
{
    [Header("Interactables")]
    [SerializeField] private XRSimpleInteractable simpleInteractable;
    [SerializeField] private XRGrabInteractable grabInteractable;

    [Header("Visuals")]
    [SerializeField] private GameObject flameObject;
    [SerializeField] private GameObject hoverText;
    
    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private float igniteAnimationDuration = 0.5f;
    [SerializeField] private bool disableAnimatorAfterIgnite = true;
    

    private Vector3 _startLocalPos;
    private bool _isIgnited;
    private bool _isPlayingIgnite;

    public bool IsIgnited => _isIgnited;
    
    private void Awake()
    {

        if (flameObject != null)
            flameObject.SetActive(false);

        if (hoverText != null)
            hoverText.SetActive(false);

        if (grabInteractable != null)
            grabInteractable.enabled = false;

        if (simpleInteractable != null)
        {
            simpleInteractable.hoverEntered.AddListener(OnHoverEntered);
            simpleInteractable.hoverExited.AddListener(OnHoverExited);
            simpleInteractable.selectEntered.AddListener(OnSelectEntered);
        }
    }

    private void OnDestroy()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.hoverEntered.RemoveListener(OnHoverEntered);
            simpleInteractable.hoverExited.RemoveListener(OnHoverExited);
            simpleInteractable.selectEntered.RemoveListener(OnSelectEntered);
        }
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (hoverText == null) return;

        hoverText.SetActive(true);
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (hoverText != null)
            hoverText.SetActive(false);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (_isPlayingIgnite)
            return;

        if (!_isIgnited)
        {
            StartCoroutine(IgniteRoutine());
        }
    }

    
    private IEnumerator IgniteRoutine()
    {
        _isPlayingIgnite = true;
        simpleInteractable.enabled = false;
        grabInteractable.enabled = false;
        animator.SetTrigger("flame");
        yield return new WaitForSeconds(igniteAnimationDuration);
        _isIgnited = true;
        if (animator != null && disableAnimatorAfterIgnite)
            animator.enabled = false;
        flameObject.SetActive(true);
        grabInteractable.enabled = true;
        _isPlayingIgnite = false;
    }
}