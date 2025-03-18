using UnityEngine;
using System.Collections;

public class PanelFade : MonoBehaviour
{
    public CanvasGroup panelCanvasGroup; // Reference to the CanvasGroup
    public float fadeDuration = 1.5f; // Duration of the fade

    void Start()
    {
        panelCanvasGroup.alpha = 0; // Ensure panel starts invisible
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            panelCanvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        panelCanvasGroup.alpha = 1; // Ensure it's fully visible at the end
    }
}

