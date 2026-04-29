using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    public float duration = 0.8f;

    void Start()
    {
        StartCoroutine(DoFade());
    }

    IEnumerator DoFade()
    {
        var img = GetComponent<Image>();
        if (img == null) yield break;

        Color c = img.color;
        c.a = 0f;
        img.color = c;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / duration);
            img.color = c;
            yield return null;
        }
    }
}
