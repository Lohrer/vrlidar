﻿using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class CreateSpheres : MonoBehaviour {
    public long numDataPoints = 1000; // Pick how many data points to display
    public int j = 0;
    public static GameObject[] dataPointArr; // Declare array of data points
    private static float maxhex = 255;

    // UDP receiving objects
    UdpClient client;
    IPEndPoint endPoint;
    public int port = 2368;
    public string hostName = "10.42.0.201";
    public GameObject car;
    public int stepNum;
    Thread listener;
    Queue pQueue = Queue.Synchronized(new Queue());

    void Start()
    {
        dataPointArr = new GameObject[numDataPoints];

        // Initialize UDP reader
        IPAddress ip = IPAddress.Any;//IPAddress.Parse(hostName);
        Debug.Log(ip.ToString());
        endPoint = new IPEndPoint(ip, port);
        client = new UdpClient(endPoint);
        listener = new Thread(new ThreadStart(Translater))
        {
            IsBackground = true
        };
        listener.Start();

        Debug.Log("Setup done");
    }

    void Update()
    {
        lock (pQueue.SyncRoot)
        {
            if (pQueue.Count > 0)
            {
                VeloPacket p = (VeloPacket)pQueue.Dequeue();
                Debug.Log("Packet received!");
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
        client.Close();
    }

    void Translater()
    {
        Byte[] data = new byte[0];
        while (true)
        {
            try
            {
                data = client.Receive(ref endPoint);
                pQueue.Enqueue(new VeloPacket(data));
            }
            catch (Exception err)
            {
                client.Close();
                return;
            }
        }
    }

    public static void update_sphere(long id, float xpos, float ypos, float zpos, float intensity)
    {
        dataPointArr[id].transform.position = new Vector3(xpos, ypos, zpos);

        Color intensityColor = new Color(0, intensity / maxhex, 0, 1);
        dataPointArr[id].gameObject.GetComponent<Renderer>().material.color = intensityColor;
    }
}public class VeloPacket
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