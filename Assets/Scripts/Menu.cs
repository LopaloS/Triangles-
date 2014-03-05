using UnityEngine;
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
    float buttonStep = 0.1f;
    Rect buttonRect;

	// Use this for initialization
	void Start () 
    {
        buttonRect = new Rect(Screen.width/2 - buttonsSize.x/2, Screen.height * 0.2f, buttonsSize.x, buttonsSize.y);
	}
	
	
	void OnGUI () 
    {
	    if(GUI.Button(buttonRect, "Start game"))
            StartGame();
	}

    void StartGame()
    {
        //Controller.Instance.Init();

        CreateControls();
    }

    void CreateControls()
    {
        var texGenerator = new CircleTextureGenerator();
        if (joystick != null)
        {
            joystick.RingTexture = texGenerator.GetTexture(256, 120, Color.white);
            joystick.JoyTexture = texGenerator.GetTexture(64, 0, Color.white);
        }

        if (button != null)
        {
            button.Texture = texGenerator.GetTexture(80, 0, Color.white);
        }
    }
}
