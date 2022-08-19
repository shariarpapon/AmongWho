using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIFadeTransition : MonoBehaviour
{
    public TweenType type;

    [Range(0.001f, 1)]
    public float timeStep;
    [Range(0.01f, 10)]
    public float velocity = 1;

    [Range(0.1f, 10)]
    public float transparecyFactor = 1;
    public bool playOnEnable = false;
    public bool applyToChildren = true;

    public AnimationCurve customCurve;

    private delegate float TweenMethod(float x);

    private void OnEnable()
    {
        if (playOnEnable)
            InitTransition();
    }

    public void InitTransition()
    {
        TweenMethod function = GetTweenMethod(type);

        if (applyToChildren)
            StartCoroutine(FadeAll(function));
        else
            StartCoroutine(Fade(function));
    }

    private IEnumerator Fade (TweenMethod function)
    {
        Image image = GetComponent<Image>();
        float x = 0;
        while (x < 1)
        {
            float alpha = function(x) * transparecyFactor;
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
            x += timeStep * velocity;
            yield return null;
        }
        Destroy(gameObject);
    }

    private IEnumerator FadeAll(TweenMethod function)
    {
        Image[] images = GetComponentsInChildren<Image>();
        float x = 0;
        while (x < 1)
        {
            float alpha = function(x) * transparecyFactor;

            foreach(Image img in images)
                img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);

            x += timeStep * velocity;
            yield return null;
        }
        Destroy(gameObject);
    }

    private TweenMethod GetTweenMethod(TweenType presetType)
    {
        TweenMethod function = null;
        switch (type)
        {
            default:
                function = new TweenMethod(FadeInMethod); break;
            case TweenType.FadeOut:
                function = new TweenMethod(FadeOutMethod); break;
            case TweenType.FadeInOut:
                function = new TweenMethod(FadeInOutMethod); break;
            case TweenType.FadeOutIn:
                function = new TweenMethod(FadeOutInMethod); break;
            case TweenType.Custom:
                function = new TweenMethod(CustomMethod); break;
        }
        return function;
    }

    private float FadeInMethod(float x)
    {
        return x;
    }
    private float FadeOutMethod(float x)
    {
        return -x + 1;
    }
    private float FadeInOutMethod(float x)
    {
        return (-4*x*x) + (4*x);
    }
    private float FadeOutInMethod(float x)
    {
        float k = (x - 1);
        return 4 * k * (k + 1) ;
    }
    private float CustomMethod(float x)
    {
        return customCurve.Evaluate(x);
    }

    [System.Serializable]
    public enum TweenType
    {
        Custom,
        FadeIn,
        FadeOut,
        FadeInOut,
        FadeOutIn
    }
}
