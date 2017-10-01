﻿using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class CreateSpheres : MonoBehaviour {
    public long numDataPoints = 12*32; // Pick how many data points to display
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
                for (int i = 0; i < numDataPoints; i++)
                {
                    UpdateSphere(i, p.x[i], p.y[i], p.z[i], 0);
                }
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
            Thread.Sleep(10);
        }
    }

    public static void UpdateSphere(long id, float xpos, float ypos, float zpos, float intensity)
    {
        dataPointArr[id].transform.position = new Vector3(xpos, ypos, zpos);
        dataPointArr[id].transform.localScale = new Vector3(1, 1, 1);

        Color intensityColor = new Color(0, intensity / maxhex, zpos % 255, 1);
        dataPointArr[id].gameObject.GetComponent<Renderer>().material.color = intensityColor;
    }
}

public class VeloPacket
{
    public float azimuth;
    public byte[] intensity;
    public float[] x;
    public float[] y;
    public float[] z;

    public VeloPacket(Byte[] data)
    {
        azimuth = 0;
        intensity = new byte[12 * 32];
        x = new float[12 * 32];
        y = new float[12 * 32];
        z = new float[12 * 32];
        ReadPacket(data);
    }

    private void ReadPacket(Byte[] data)
    {
        // Read all 12 azimuths
        float[] a = new float[24];
        a[0] = BitConverter.ToUInt16(data, 2) / 100f;
        azimuth = a[0];
        for (int i = 1; i < 11; i++)
        {
            a[i*2] = BitConverter.ToUInt16(data, i*100+2) / 100f;
            a[i * 2 - 1] = (a[i * 2 - 2] + a[i * 2]) / 2;
        }
        int[] w = {-15, 1, -13, 3, -11, 5, -9, 7, -7, 9, -5, 11, -3, 13, -1, 15};
        for (int i = 0; i < 12*32; i++)
        {
            int index = 100 * (i / 32) + 3 * (i % 32) + 4;
            float r = BitConverter.ToUInt16(data, index) / 500f;
            intensity[i] = data[index + 2];
            y[i] = r * (float)Math.Cos(w[i % 16]) * (float)Math.Sin(a[i / 16]);
            z[i] = -r * (float)Math.Cos(w[i % 16]) * (float)Math.Cos(a[i / 16]);
            x[i] = r * (float)Math.Sin(w[i % 16]);
        }
    }
}
