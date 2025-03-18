using UnityEngine;
using System.Collections;

public class PanelAnimator : MonoBehaviour
{
    public CanvasGroup panelCanvasGroup;
    public RectTransform panelTransform;
    public float animationDuration = 0.5f;

    void Start()
    {
        panelCanvasGroup.alpha = 0;
        panelTransform.localScale = Vector3.zero;
    }

    public void ShowPanel()
    {
        StartCoroutine(FadeAndScale(panelCanvasGroup, panelTransform, 0, 1, animationDuration));
    }

    public void HidePanel()
    {
        StartCoroutine(FadeAndScale(panelCanvasGroup, panelTransform, 1, 0, animationDuration));
    }

    IEnumerator FadeAndScale(CanvasGroup canvasGroup, RectTransform transform, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startScale = (startAlpha == 0) ? Vector3.zero : Vector3.one;
        Vector3 endScale = (endAlpha == 0) ? Vector3.zero : Vector3.one;

        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            transform.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        transform.localScale = endScale;
    }
}
