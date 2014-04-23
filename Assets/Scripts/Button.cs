using UnityEngine;
using System.Collections;

public class Button : ControllInput
{
    public Texture Texture
    {
        get { return texture; }
        set { texture = value; }
    }
    public bool IsPresed {get; set;}

    [SerializeField]
    Texture texture;

    [SerializeField]
    Vector2 size = new Vector2(0.1f, 0.1f);

    Vector2 sizeInPixels;

    Rect rect;

    void Start()
    {
        Init();
        IsPresed = false;
        sizeInPixels = size * Screen.height;
        rect = new Rect(0, 0, sizeInPixels.x, sizeInPixels.y);
        rect.center = posOnScreenInPixels;
    }

    void OnGUI()
    {
        GUI.Label(rect, texture);
        if (rect.Contains(InvertYpos))
            IsPresed = true;
        else
            IsPresed = false;
    }
}
