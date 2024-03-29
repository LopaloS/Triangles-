﻿using UnityEngine;
using System.Collections;

public class Joystick : ControllInput
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
    public event System.Action<Vector2> SendPosition;

    [SerializeField]
    Texture joyTexture;

    [SerializeField]
    Texture ringTexture;

    [SerializeField]
    float ringSize = 0.1f;

    [SerializeField]
    float joySize = 0.04f;

    Rect ringRect;
    
    float halfRectSizePixels = 40;

    Vector2 joyRectOffset;
    Rect joyRect;
    Vector2 touchPosOnScreen;

    // Use this for initialization
    void OnEnable()
    {
        Init();
        float ringRectSizePixels = Screen.width * ringSize;
        halfRectSizePixels = ringRectSizePixels / 2;
        ringRect = new Rect(0, 0, ringRectSizePixels, ringRectSizePixels);
        ringRect.center = posOnScreenInPixels;
        
        float joyRectSizeInPixels = Screen.width * joySize;
        joyRect = new Rect(0, 0, joyRectSizeInPixels, joyRectSizeInPixels);
        joyRect.center = posOnScreenInPixels;
    }


    void OnGUI()
    {
        GUI.Label(ringRect, ringTexture);
        if (Position != Vector2.zero)
        {
            joyRect.center = touchPosOnScreen;
        }
        else
            joyRect.center = posOnScreenInPixels;
        GUI.Label(joyRect, joyTexture);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i <  TouchPositions.Length; i++)
        {
            Vector2 posRelJoyCenter = new Vector2(posOnScreenInPixels.x, Screen.height - posOnScreenInPixels.y) - TouchPositions[i];
            if (posRelJoyCenter.magnitude > halfRectSizePixels)
                Position = Vector2.zero;
            else
                Position = -posRelJoyCenter / halfRectSizePixels;
#if UNITY_EDITOR || UNITY_STANDALONE
            if (!Input.GetMouseButton(0))
                Position = Vector2.zero;
#endif
            if (Position != Vector2.zero)
            {
                if (SendPosition != null)
                    SendPosition(Position);
                touchPosOnScreen = InvertYTouchPositions[i];
                break;
            }
        }
    }
}
