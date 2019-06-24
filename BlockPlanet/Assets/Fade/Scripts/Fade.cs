using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class Fade : SingletonMonoBehaviour<Fade>
{
    IFade fade;

    bool is_end = true;
    public bool IsEnd { get { return is_end; } }

    void Start()
    {
        Init();
        fade.Range = cutoutRange;
    }

    float cutoutRange = 0;

    void Init()
    {
        fade = GetComponent<IFade>();
    }

    void OnValidate()
    {
        Init();
        fade.Range = cutoutRange;
    }

    IEnumerator FadeoutCoroutine(float time, System.Action action)
    {
        // cutoutRange = 1;
        float endTime = Time.timeSinceLevelLoad + time * (cutoutRange);

        var endFrame = new WaitForEndOfFrame();

        while (Time.timeSinceLevelLoad <= endTime)
        {
            cutoutRange = (endTime - Time.timeSinceLevelLoad) / time;
            fade.Range = cutoutRange;
            yield return endFrame;
        }
        cutoutRange = 0;
        fade.Range = cutoutRange;

        if (action != null)
        {
            action();
        }

        is_end = true;
    }

    IEnumerator FadeinCoroutine(float time, System.Action action)
    {
        // cutoutRange = 0;
        float endTime = Time.timeSinceLevelLoad + time * (1 - cutoutRange);

        var endFrame = new WaitForEndOfFrame();

        while (Time.timeSinceLevelLoad <= endTime)
        {
            cutoutRange = 1 - ((endTime - Time.timeSinceLevelLoad) / time);
            fade.Range = cutoutRange;
            yield return endFrame;
        }
        cutoutRange = 1;
        fade.Range = cutoutRange;

        if (action != null)
        {
            action();
        }

        is_end = true;
    }

    public Coroutine FadeOut(float time, System.Action action)
    {
        if (!is_end) return null;
        is_end = false;
        StopAllCoroutines();
        return StartCoroutine(FadeoutCoroutine(time, action));
    }

    public Coroutine FadeOut(float time)
    {
        return FadeOut(time, null);
    }

    public Coroutine FadeIn(float time, System.Action action)
    {
        if (!is_end) return null;
        is_end = false;
        StopAllCoroutines();
        return StartCoroutine(FadeinCoroutine(time, action));
    }

    public Coroutine FadeIn(float time)
    {
        return FadeIn(time, null);
    }
}