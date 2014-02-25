using UnityEngine;
using System.Collections;
using WebSocketSharp;

public class Controller
{
    static public Controller Instance
    {
        get
        {
            if (instance == null)
                instance = new Controller();
            return instance;
        }
    }
    static Controller instance;

    Controller()
    {
    }

    public void Init()
    {
        WebSocket ws = new WebSocket("ws://89.252.17.39:9000/");

        ws.OnOpen += (o, e) =>
        {
            Debug.Log("Open");
        };
        ws.Connect();
    }

}

public class BlackBox
{
    Vector2 pos = new Vector2(100, 100);
    float angle = 0;

    public void Handle(object cmd, System.EventArgs args)
    {
        switch ((string)cmd)
        {
            case "world.start":

                break;
            case "user.commands":

                break;
            case "world.get_objects_info":
                break;
        }
    }
}
