using UnityEngine;
using System.Collections;

public class Button : ControllInput
{
    public Texture Texture
    {
        get { return texture; }
        set { texture = value; }
    }
    public bool IsPresed { get; set; }

    [SerializeField]
    Texture texture;

    [SerializeField]
    Vector2 size = new Vector2(0.2f, 0.2f);

    Vector2 sizeInPixels;
    Rect rect;

    void OnEnable()
    {
        Init();
        IsPresed = false;
        sizeInPixels = size * Screen.width;
        rect = new Rect(0, 0, sizeInPixels.x, sizeInPixels.y);
        rect.center = posOnScreenInPixels;
    }

    void OnGUI()
    {
        GUI.Label(rect, texture);
#if UNITY_EDITOR || UNITY_STANDALONE
        if (!Input.GetMouseButton(0))
        {
            IsPresed = false;
            return;
        }
#endif
        foreach (var pos in InvertYTouchPositions)
        {
            if (rect.Contains(pos))
            {
                IsPresed = true;
                return;
            }
        }
        IsPresed = false;
    }
}
