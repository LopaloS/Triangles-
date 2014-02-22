using UnityEngine;
using System.Collections;

public class Controller
{
    public Controller Instance
    {
        get
        {
            if (instance == null)
                instance = new Controller();
            return instance;
        }
    }
    Controller instance;

    Controller()
    {
    }
}
