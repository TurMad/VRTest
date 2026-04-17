using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneIntroController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup paperGroup;
    [SerializeField] private CanvasGroup textGroup;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Audio")]
    [SerializeField] private AudioClip voiceClip;

    [Header("Timing")]
    [SerializeField] private float sceneFadeDelay = 0.2f;
    [SerializeField] private float paperFadeDuration = 0.5f;
    [SerializeField] private float textFadeDuration = 0.4f;
    [SerializeField] private float startAudioDelay = 0.2f;
    [SerializeField] private float endPause = 0.5f;
    [SerializeField] private float hideDuration = 0.5f;

    [Header("Next Step")]
    [SerializeField] private GameObject[] activateAfterIntro;

    private void Start()
    {
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        SceneFlowManager.Instance.SetXRLocked(true);
        SceneFlowManager.Instance.SetMoveTurnLocked(true);

        paperGroup.alpha = 0f;
        textGroup.alpha = 0f;
        scrollRect.verticalNormalizedPosition = 1f;

        yield return new WaitForSeconds(sceneFadeDelay);

        yield return FadeCanvasGroup(paperGroup, 0f, 1f, paperFadeDuration);
        yield return FadeCanvasGroup(textGroup, 0f, 1f, textFadeDuration);

        yield return new WaitForSeconds(startAudioDelay);

        SceneFlowManager.Instance.PlayAudio(voiceClip);
        yield return StartCoroutine(AutoScrollByAudio());

        yield return new WaitForSeconds(endPause);

        StartCoroutine(FadeCanvasGroup(textGroup, textGroup.alpha, 0f, hideDuration));
        yield return FadeCanvasGroup(paperGroup, paperGroup.alpha, 0f, hideDuration);

        SceneFlowManager.Instance.SetObjectsActive(activateAfterIntro, true);

        SceneFlowManager.Instance.SetMoveTurnLocked(false);
        SceneFlowManager.Instance.SetXRLocked(false);
    }

    private IEnumerator AutoScrollByAudio()
    {
        while (SceneFlowManager.Instance.IsAudioPlaying)
        {
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(1f, 0f, SceneFlowManager.Instance.AudioProgress);
            yield return null;
        }

        scrollRect.verticalNormalizedPosition = 0f;
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
}