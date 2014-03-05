using UnityEngine;
using System.Collections;

public class ControllInput : MonoBehaviour
{
    public Vector2 Pos
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            pos = Input.mousePosition;
#else
        if (Input.touches.Length == 0) return;
        var touch = Input.touches[0];
        pos = touch.position;
#endif
            return pos;
        }
    }
    protected Vector2 InvertYpos
    {
        get
        {
            return new Vector2(Pos.x, Screen.height - Pos.y);
        }
    }

    [SerializeField]
    protected Vector2 posOnScreen = new Vector2(0.1f, 0.1f);

    protected Vector2 posOnScreenInPixels;
    Vector2 pos = Vector2.zero;

    protected void Init()
    {
        posOnScreenInPixels = new Vector2(Screen.width * posOnScreen.x, Screen.height * posOnScreen.y);
    }
}
