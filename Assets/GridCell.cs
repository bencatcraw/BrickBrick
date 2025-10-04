using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GridCell : MonoBehaviour
{
    public Vector2Int gridPosition;
    public bool isOccupied = false;

    [Header("Colors")]
    public Color normalColor;

    public Image blockImage;

    private Color lastColor;
    private Coroutine pulseCoroutine;
    bool isGlowing = false;

    void Awake()
    {
        ResetHighlight();
    }

    public void ToggleBlock(bool toggle, Color color)
    {
        if (toggle)
        {
            isOccupied = true;
            blockImage.color = color;
        }
        else
        {
            isOccupied = false;
            blockImage.color = normalColor;
        }
    }

    public void Highlight(Color hoverColor)
    {
        hoverColor.a /= 3;
        if (!isGlowing && blockImage != null)
            blockImage.color = hoverColor;
    }

    public void ResetHighlight()
    {
        if (!isGlowing && blockImage != null)
            blockImage.color = normalColor;
    }

    public void GlowToggle(bool toggle, Color glowColor)
    {
        if (toggle)
        {
            if (!isGlowing)
            {
                isGlowing = true;
                lastColor = blockImage.color;
                blockImage.color = glowColor;
                pulseCoroutine = StartCoroutine(Pulse());
            }
        }
        else
        {
            if (isGlowing)
            {
                isGlowing = false;
                if (pulseCoroutine != null)
                {
                    StopCoroutine(pulseCoroutine);
                    pulseCoroutine = null;
                }
                blockImage.color = lastColor;
            }
        }
    }

    IEnumerator Pulse()
    {
        float duration = 1f;
        float time = 0f;

        while (true)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Abs(Mathf.Sin(time * Mathf.PI / duration));
            Color c = blockImage.color;
            c.a = Mathf.Lerp(0.3f, 1f, alpha);
            blockImage.color = c;
            yield return null;
        }
    }
}
