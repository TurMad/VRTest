using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class IntroVolumeFade : MonoBehaviour
{
    [SerializeField] private Volume fadeVolume;
    [SerializeField] private float delay = 0.2f;
    [SerializeField] private float duration = 1.2f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);

        float time = 0f;
        fadeVolume.weight = 1f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            fadeVolume.weight = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        fadeVolume.weight = 0f;
    }
}