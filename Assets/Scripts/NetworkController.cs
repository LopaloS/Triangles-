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

    public event Action<Dictionary<string, object>> UpdateScene;
    public event Action OnInit;

    public Dictionary<string, object> Data
    {
        get
        {
            var dataQueue = Queue.Synchronized(NetworkController.Instance.dataQueue);
            if (dataQueue.Count == 0) return null;
            var obj = dataQueue.Dequeue();
            if (obj == null) return null;
            return (Dictionary<string, object>)obj;
        }
    }

    JsonReader jsonReader;
    JsonWriter jsonWriter; 
    WebSocket webSocket;
    Queue dataQueue;

    FakeConection fakeConection;
    bool isInit = false;
    bool debug = false;

    NetworkController()
    {
        dataQueue = new Queue();
    }

    public void Init(string url, string playerName)
    {
        webSocket = new WebSocket(url);

        webSocket.OnOpen += (o, e) =>
        {
            Debug.Log("Open");

            var dataDict = new Dictionary<string, object>();
            dataDict.Add("cmd", "world.start");
            var nameDict = new Dictionary<string, string>();
            nameDict.Add("name", playerName);
            dataDict.Add("args", nameDict);

            jsonWriter = new JsonWriter();
            jsonReader = new JsonReader();
            webSocket.Send(jsonWriter.Write(dataDict));
        };
        webSocket.OnMessage += OnMessageHandler;
        webSocket.Connect();


        if (debug)
        {
            var fakeConectionGo = new GameObject("FakeConnection", typeof(FakeConection));
            fakeConection = fakeConectionGo.GetComponent<FakeConection>();

            fakeConection.OnMessage += OnMessageHandler;
        }
    }

    public void CloseConnection()
    {
        if (webSocket != null)
            webSocket.Close();
    }

    public void SendMoveVector(Vector2 pos, bool isFire)
    {
        float[] vec = new float[2];
        vec[0] = pos.magnitude;
        vec[1] = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg;
        var message = new Dictionary<string, object>();
        message.Add("cmd", "user.commands");
        var args = new Dictionary<string, object>();
        args.Add("move_vector", vec);
        args.Add("fire", isFire);
        message.Add("args", args);

        SendObject(message);
    }

    public void RequestUnknownObjs(List<string> ids)
    {
        var message = new Dictionary<string, object>();
        message.Add("cmd", "world.get_objects_info");
        var identsDict = new Dictionary<string, object>();
        identsDict.Add("idents", ids);
        message.Add("args", identsDict);

        SendObject(message);
    }

    void SendObject(object message)
    {
        string stringMessage = jsonWriter.Write(message);

        if (webSocket == null) return;
        webSocket.Send(stringMessage);
        if (debug && fakeConection != null)
            fakeConection.Send(stringMessage);
    }

    void OnMessageHandler(object sender, MessageEventArgs args)
    {
        string data = args.Data;
        var tempDict = jsonReader.Read<Dictionary<string, object>>(data);
        
        switch ((string)tempDict["cmd"])
        {
            case "world.init":
                if (OnInit != null)
                {
                    dataQueue.Enqueue(tempDict);
                    OnInit();
                    isInit = true;
                }
                break;
            case "world.tick":
            case "scores.update":
            case "world.objects_info":
                if(isInit)
                    dataQueue.Enqueue(tempDict);
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


