using UnityEngine;
using UnityEngine.UI;

public class BlockUnit : MonoBehaviour
{
    public Vector2Int offset;
    public void setColor(Color color)
    {
        GetComponent<Image>().color = color;
    }
}
