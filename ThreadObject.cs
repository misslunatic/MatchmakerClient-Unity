using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.CustomServer;

public class ThreadObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ThreadManager.Start();
    }

    // Update is called once per frame
    void Update()
    {
        ThreadManager.UpdateMain();
    }
}
