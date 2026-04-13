using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class BookInteractable : MonoBehaviour
{
    [Serializable]
    public class BookSpread
    {
        [TextArea(3, 20)] public string leftPageText;
        [TextArea(3, 20)] public string rightPageText;
    }

    [Header("View")]
    [SerializeField] private Transform viewPoint;
    [SerializeField] private float moveDuration = 0.35f;

    [Header("Pages UI")]
    [SerializeField] private CanvasGroup pagesGroup;
    [SerializeField] private TMP_Text leftPageText;
    [SerializeField] private TMP_Text rightPageText;
    [SerializeField] private float pagesFadeDuration = 0.2f;

    [Header("Animator")]
    [SerializeField] private Animator bookAnimator;
    [SerializeField] private string pageTurnTriggerName = "NextPage";
    [SerializeField] private float pageTurnWaitTime = 0.35f;

    [Header("Input")]
    [SerializeField] private InputActionReference[] openActions;
    [SerializeField] private InputActionReference[] nextPageActions;
    [SerializeField] private float inputDelayAfterOpen = 0.25f;

    [Header("Content")]
    [SerializeField] private BookSpread[] spreads;

    [Header("Highlight")]
    [SerializeField] private InteractableHighlight highlight;

    private XRSimpleInteractable interactable;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private int currentSpreadIndex;
    private bool isOpen;
    private bool isAnimating;
    private float ignoreInputUntil;
    private int hoverCount;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();

        if (highlight == null)
            highlight = GetComponent<InteractableHighlight>();

        startPosition = transform.position;
        startRotation = transform.rotation;

        if (pagesGroup != null)
        {
            pagesGroup.alpha = 0f;
            pagesGroup.blocksRaycasts = false;
            pagesGroup.interactable = false;
        }
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelected);
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);

        EnableActions(openActions, true);
        EnableActions(nextPageActions, true);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelected);
        interactable.hoverEntered.RemoveListener(OnHoverEntered);
        interactable.hoverExited.RemoveListener(OnHoverExited);

        EnableActions(openActions, false);
        EnableActions(nextPageActions, false);
    }

    private void Update()
    {
        if (isAnimating)
            return;

        if (!isOpen)
        {
            if (hoverCount > 0 && WasAnyActionPressedThisFrame(openActions))
                TryOpenBook();

            return;
        }

        if (Time.time < ignoreInputUntil)
            return;

        if (!WasAnyActionPressedThisFrame(nextPageActions))
            return;

        if (currentSpreadIndex < spreads.Length - 1)
            StartCoroutine(NextSpreadRoutine());
        else
            StartCoroutine(CloseBookRoutine());
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        if (!isOpen)
            TryOpenBook();
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        hoverCount++;
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        hoverCount = Mathf.Max(0, hoverCount - 1);
    }

    private void TryOpenBook()
    {
        if (isOpen || isAnimating || spreads == null || spreads.Length == 0 || viewPoint == null)
            return;

        StartCoroutine(OpenBookRoutine());
    }

    private IEnumerator OpenBookRoutine()
    {
        isAnimating = true;

        if (highlight != null)
            highlight.StopHighlight();

        startPosition = transform.position;
        startRotation = transform.rotation;

        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        yield return MoveTransform(transform, startPosition, startRotation, viewPoint.position, viewPoint.rotation, moveDuration);

        currentSpreadIndex = 0;
        ApplySpread(currentSpreadIndex);

        if (pagesGroup != null)
            yield return FadeCanvasGroup(pagesGroup, 0f, 1f, pagesFadeDuration);

        isOpen = true;
        isAnimating = false;
        ignoreInputUntil = Time.time + inputDelayAfterOpen;
    }

    private IEnumerator NextSpreadRoutine()
    {
        isAnimating = true;

        if (pagesGroup != null)
            yield return FadeCanvasGroup(pagesGroup, 1f, 0f, pagesFadeDuration);

        if (bookAnimator != null && !string.IsNullOrEmpty(pageTurnTriggerName))
            bookAnimator.SetTrigger(pageTurnTriggerName);

        yield return new WaitForSeconds(pageTurnWaitTime);

        currentSpreadIndex++;
        ApplySpread(currentSpreadIndex);

        if (pagesGroup != null)
            yield return FadeCanvasGroup(pagesGroup, 0f, 1f, pagesFadeDuration);

        isAnimating = false;
        ignoreInputUntil = Time.time + 0.1f;
    }

    private IEnumerator CloseBookRoutine()
    {
        isAnimating = true;
        isOpen = false;

        if (pagesGroup != null)
            yield return FadeCanvasGroup(pagesGroup, 1f, 0f, pagesFadeDuration);

        yield return MoveTransform(transform, transform.position, transform.rotation, startPosition, startRotation, moveDuration);

        SceneFlowManager.Instance.SetMoveTurnLocked(false);

        isAnimating = false;
    }

    private void ApplySpread(int index)
    {
        if (index < 0 || index >= spreads.Length)
            return;

        if (leftPageText != null)
            leftPageText.text = spreads[index].leftPageText;

        if (rightPageText != null)
            rightPageText.text = spreads[index].rightPageText;
    }

    private bool WasAnyActionPressedThisFrame(InputActionReference[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            if (actions[i] != null && actions[i].action.WasPressedThisFrame())
                return true;
        }

        return false;
    }

    private void EnableActions(InputActionReference[] actions, bool enable)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            if (actions[i] == null)
                continue;

            if (enable)
                actions[i].action.Enable();
            else
                actions[i].action.Disable();
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        float time = 0f;
        group.alpha = from;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(time / duration);
            group.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        group.alpha = to;
    }

    private IEnumerator MoveTransform(Transform target, Vector3 fromPos, Quaternion fromRot, Vector3 toPos, Quaternion toRot, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(time / duration);
            target.position = Vector3.Lerp(fromPos, toPos, t);
            target.rotation = Quaternion.Slerp(fromRot, toRot, t);
            yield return null;
        }

        target.position = toPos;
        target.rotation = toRot;
    }
}