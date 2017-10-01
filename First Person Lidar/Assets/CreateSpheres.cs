using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class CreateSpheres : MonoBehaviour {
    public long numDataPoints = 1000; // Pick how many data points to display
    public static GameObject[] dataPointArr; // Declare array of data points
    private static float maxhex = 255;

    // UDP receiving objects
    public int port = 2368;
    Thread listener;
    Queue pQueue = Queue.Synchronized(new Queue());

    void Start()
    {
        dataPointArr = new GameObject[numDataPoints];

        long i; // Counter
        for (i = 0; i < numDataPoints; i++)
        {
            dataPointArr[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dataPointArr[i].transform.position = new Vector3(0, 0, 0);
        }

        // Initialize UDP reader thread
        listener = new Thread(new ThreadStart(Translater))
        {
            IsBackground = true
        };
        listener.Start();
    }

    void Update()
    {
        lock (pQueue.SyncRoot)
        {
            if (pQueue.Count > 0)
            {
                VeloPacket p = (VeloPacket)pQueue.Dequeue();
                //Debug.Log((string)pQueue.Dequeue());
                // Draw circles here
                //UpdateSphere(j, j, j, j);
            }
        }
    }

    void OnApplicationQuit()
    {
        if (listener != null && listener.IsAlive)
        {
            listener.Abort();
        }
    }

    void Translater()
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
        UdpClient client = new UdpClient(port);
        Byte[] data;
        while (true)
        {
            try
            {
                data = client.Receive(ref endPoint);
                pQueue.Enqueue(new VeloPacket(data));
                //pQueue.Enqueue(endPoint.Address + ":" + endPoint.Port);
            }
            catch
            {
                client.Close();
                return;
            }
            Thread.Sleep(100);
        }
    }

    public static void UpdateSphere(long id, float xpos, float ypos, float zpos, float intensity)
    {
        dataPointArr[id].transform.position = new Vector3(xpos, ypos, zpos);

        Color intensityColor = new Color(0, intensity / maxhex, 0, 1);
        dataPointArr[id].gameObject.GetComponent<Renderer>().material.color = intensityColor;
    }
}

public class VeloPacket
{
    Byte[] raw;

    public VeloPacket(Byte[] data)
    {
        data.CopyTo(raw, 0);
        ReadPacket(data);
    }

    private void ReadPacket(Byte[] data)
    {
        // Parse header
        UInt16 Channel0r = 0;
        float Channel0r2 = Channel0r / 500f;
    }
}
