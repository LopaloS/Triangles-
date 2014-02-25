using UnityEngine;
using System.Collections;

public class Joystick : MonoBehaviour
{
    public Texture JoyTexture
    {
        get { return joyTexture; }
        set { joyTexture = value; }
    }
    public Texture RingTexture
    {
        get { return ringTexture; }
        set { ringTexture = value; }
    }

    public Vector2 Position { get; private set; }
    public float Angle { get; private set; }

    [SerializeField]
    Texture joyTexture;

    [SerializeField]
    Texture ringTexture;

    [SerializeField]
    Vector2 posOnScreen = new Vector2(0.1f, 0.1f);

    [SerializeField]
    float size = 0.1f;

    Rect joyRect;
    Vector2 posOnScreenInPixels;
    float rectSizePixels;
    float halfRectSizePixels;


    // Use this for initialization
    void Start()
    {
        float halfSize = size * 0.5f;
        posOnScreenInPixels = new Vector2(Screen.width * posOnScreen.x, Screen.height * posOnScreen.y);

        Vector2 rectOffset = new Vector2(halfSize, -halfSize) * Screen.height;
        rectSizePixels = Screen.height * size;
        halfRectSizePixels = rectSizePixels * 0.5f;

        Vector2 rectPos = posOnScreenInPixels - rectOffset;
        joyRect = new Rect(rectPos.x, Screen.height - rectPos.y, rectSizePixels, rectSizePixels);
    }


    void OnGUI()
    {
        GUI.Label(joyRect, joyTexture);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = Vector2.zero;
#if UNITY_EDITOR || UNITY_STANDALONE
        pos = Input.mousePosition;
#else
        if (Input.touches.Length == 0) return;
        var touch = Input.touches[0];
        pos = touch.position;
#endif
        if (pos == Vector2.zero)
        {
            Position = Vector2.zero;
            return;
        }

        Vector2 posRelJoyCenter = -(posOnScreenInPixels - pos);
        if (posRelJoyCenter.magnitude > halfRectSizePixels)
        {
            Position = Vector2.zero;
            return;
        }
        else
            Position = posRelJoyCenter / halfRectSizePixels;

        print(Position);
    }
}
