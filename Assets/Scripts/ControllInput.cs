using UnityEngine;
using System.Collections;

public class ControllInput : MonoBehaviour
{
    protected Vector2[] TouchPositions
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return new Vector2[]{ Input.mousePosition};
#else
        if (Input.touches.Length == 0) 
            return new Vector2[]{ Vector2.zero};
        Vector2[] touches = new Vector2[Input.touches.Length];
        for (int i = 0; i < Input.touches.Length; i++ )
        {
            touches[i] = Input.touches[i].position;
        }
        return touches;
#endif
        }
    }

    protected Vector2[] InvertYTouchPositions
    {
        get
        {
            Vector2[] invertYPosArr = new Vector2[TouchPositions.Length];
            for (int i = 0; i < TouchPositions.Length; i++)
            {
                invertYPosArr[i] = new Vector2(TouchPositions[i].x, Screen.height - TouchPositions[i].y);
            }
            return invertYPosArr;
        }
    }

    [SerializeField]
    protected Vector2 posOnScreen = new Vector2(0.1f, 0.1f);

    protected Vector2 posOnScreenInPixels;

    protected void Init()
    {
        posOnScreenInPixels = new Vector2(Screen.width * posOnScreen.x, Screen.height * posOnScreen.y);
    }
}
