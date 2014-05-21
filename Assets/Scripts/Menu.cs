﻿using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour 
{
    [SerializeField]
    Joystick joystick;

    [SerializeField]
    Button button;

    [SerializeField]
    WorldDrawer worldDrawer;

    Vector2 buttonsSize = new Vector2(200, 50);
    Vector2 nameLabelSize = new Vector2(40, 30);
    Vector2 nameFieldSize = new Vector2(200, 25);
    Vector2 addressLabelSize = new Vector2(50, 30);
    Vector2 addressFieldSize = new Vector2(400, 25);

    Rect buttonRect;
    Rect nameLabel;
    Rect nameField;
    Rect addressLabel;
    Rect addressField;

    string address = "ws://localhost:9000/"; //"ws://89.252.17.39:9000/";
    string playerName = string.Empty;
    bool isInit = false;
    bool startGame = false;

	// Use this for initialization
	void Start () 
    {
        addressLabel = GetWidgetRect(addressLabelSize, 0.1f);
        addressField = GetWidgetRect(addressFieldSize, 0.14f);
        nameLabel = GetWidgetRect(nameLabelSize, 0.2f);
        nameField = GetWidgetRect(nameFieldSize, 0.24f);
        
        buttonRect = GetWidgetRect(buttonsSize, 0.5f);
	}

    Rect GetWidgetRect(Vector2 size, float yPos)
    {
        return new Rect(Screen.width / 2 - size.x / 2, Screen.height * yPos, size.x, size.y);
    }
	
	void OnGUI () 
    {
        if (!startGame)
        {
            GUI.Label(addressLabel, "Address");
            address = GUI.TextField(addressField, address);
            playerName = GUI.TextField(nameField, playerName);

            GUI.Label(nameLabel, "Name");

            if (GUI.Button(buttonRect, "Connect"))
                StartGame();
        } 
	}
    IEnumerator WaitInit()
    {
        NetworkController.Instance.OnInit += Init;
        while (!isInit)
            yield return null;

        CreateControls();
        worldDrawer.Init();
    }

    void Init()
    {
        isInit = true;
    }

    void StartGame()
    {
        StartCoroutine(WaitInit());
        NetworkController.Instance.Init(address, playerName);
        startGame = true;
    }

    void CreateControls()
    {
        var texGenerator = new CircleTextureGenerator();
        if (joystick != null)
        {
            joystick.RingTexture = texGenerator.GetTexture(256, 120, Color.white);
            joystick.JoyTexture = texGenerator.GetTexture(64, 0, Color.white);
            joystick.SendPosition += NetworkController.Instance.SendMoveVector;
        }

        if (button != null)
        {
            button.Texture = texGenerator.GetTexture(80, 0, Color.white);
        }
    }

    void OnDestroy()
    {
        NetworkController.Instance.CloseConnection();
    }
}
