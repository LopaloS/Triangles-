using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WebSocketSharp;
using System;
using JsonFx.Json;

public class NetworkController
{
    static public NetworkController Instance
    {
        get
        {
            if (instance == null)
                instance = new NetworkController();
            return instance;
        }
    }
    static NetworkController instance;

    public Action<Dictionary<string, object>> UpdateScene;

    JsonReader jsonReader;
    WebSocket webSocket;

    FakeConection fakeConection;
    bool debug = true;

    NetworkController()
    {
    }

    public void Init()
    {
        //webSocket = new WebSocket("ws://89.252.17.39:9000/");

        //webSocket.OnOpen += (o, e) =>
        //{
        //    Debug.Log("Open");
        //};
        //webSocket.OnMessage += OnMessageHandler; 
        //webSocket.Connect();

        if (debug)
        {
            var fakeConectionGo = new GameObject("FakeConnection", typeof(FakeConection));
            fakeConection = fakeConectionGo.GetComponent<FakeConection>();

            fakeConection.OnMessage += OnMessageHandler;
        }
    }

    public void SendMoveVector(Vector2 pos)
    {
        float[] vec = new float[2];
        vec[0] = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
        vec[1] = pos.magnitude;
        var message = new Dictionary<string, object>();
        message.Add("cmd", "user.commands");
        var args = new Dictionary<string, object>();
        args.Add("move_vector", vec);
        message.Add("args", args);

        var writer = new JsonWriter();
        string stringMessage = writer.Write(message);
        //webSocket.Send(stringMessage);
        if (debug && fakeConection != null)
            fakeConection.Send(stringMessage);
    }

    void OnMessageHandler(object sender, MessageEventArgs args)
    {
        string data = args.Data;
        var tempDict = jsonReader.Read(data, typeof(Dictionary<string, object>)) as Dictionary<string, object>;

        switch ((string)tempDict["cmd"])
        {
            case "world.init":
                break;
        }
    }

}

[Serializable]
public class JsonVector2
{
    public JsonVector2() { }
    public JsonVector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
    public float x;
    public float y;
}


