using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

public class CreateSpheres : MonoBehaviour {
    public List<GameObject> cubes; // Declare array of data points
    private static float maxhex = 255;
    private static int point = 0;
    private static float lastAzimuth = 0;
    private List<VeloPacket> cloud;

    // UDP receiving objects
    public int port = 2368;
    Thread listener;
    Queue pQueue = Queue.Synchronized(new Queue());

    void Start()
    {
        cubes = new List<GameObject>(100000);
        cloud = new List<VeloPacket>(100000);

        //long i; // Counter
        /*for (i = 0; i < numDataPoints; i++)
        {
            dataPointArr[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dataPointArr[i].transform.position = new Vector3(0, 0, 0);
            dataPointArr[i].transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
        }*/

        // Initialize UDP reader thread
        listener = new Thread(new ThreadStart(Translater))
        {
            IsBackground = true
        };
        listener.Start();
    }

    void Update()
    {
        bool redraw = false;
        lock (pQueue.SyncRoot)
        {
            if (pQueue.Count > 0)
            {
                VeloPacket p = (VeloPacket)pQueue.Dequeue();
                //cloud.Add(p);
                //if (p.azimuth < lastAzimuth)
                //{
                //    redraw = true;
                //}
                //lastAzimuth = p.azimuth;
                for (int i = 0; i < p.x.Length; i++)
                {
                    GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    g.transform.position = new Vector3(p.x[i], p.y[i], p.z[i]);
                    g.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                    g.gameObject.GetComponent<Renderer>().material.color = new Color(0, p.y[i] % 256, 0, 1);
                    Destroy(g, 20);
                }
            }
        }

        /*if (redraw)
        {
            // Redraw pointcloud
            redraw = false;
            point = 0;
            cubes.Clear();
            foreach (VeloPacket p in cloud)
            {
                for (int i = 0; i < p.x.Length; i++)
                {
                    //UpdateSphere(point + i, p.x[i], p.y[i], p.z[i], p.z[i]);
                    GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    g.transform.position = new Vector3(p.x[i], p.y[i], p.z[i]);
                    g.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                    g.gameObject.GetComponent<Renderer>().material.color = new Color(0, p.z[i] % 256, 0, 1);
                    cubes.Add(g);
                }
                point += p.x.Length;
            }

            cloud.Clear();
        }*/

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
        }
    }

    /*public static void UpdateSphere(long id, float xpos, float ypos, float zpos, float intensity)
    {
        dataPointArr[id].transform.position = new Vector3(xpos, ypos, zpos);

        Color intensityColor = new Color(0, intensity % 256, 0, 1);
        dataPointArr[id].gameObject.GetComponent<Renderer>().material.color = intensityColor;
    }*/
}

public class VeloPacket
{
    public float azimuth;
    public byte[] intensity;
    public float[] x;
    public float[] y;
    public float[] z;
    public float[] r;

    public VeloPacket(Byte[] data)
    {
        azimuth = 0;
        intensity = new byte[12 * 32];
        x = new float[12 * 32];
        y = new float[12 * 32];
        z = new float[12 * 32];
        r = new float[12 * 32];
        ReadPacket(data);
    }

    private void ReadPacket(Byte[] data)
    {
        // Read all 12 azimuths
        float[] a = new float[24];
        a[0] = BitConverter.ToUInt16(data, 2) * (float)Math.PI / 100f / 180f;
        azimuth = a[0];
        float[] w = {-15f, 1f, -13f, 3f, -11f, 5f, -9f, 7f, -7f, 9f, -5f, 11f, -3f, 13f, -1f, 15f};
        for (int i = 0; i < w.Length; i++)
        {
            w[i] = w[i] * (float)Math.PI / 180f; // Convert to radians
        }
        for (int i = 1; i < 11; i++)
        {
            a[i*2] = BitConverter.ToUInt16(data, i*100+2) * (float)Math.PI / 100f / 180f;
            a[i * 2 - 1] = (a[i * 2 - 2] + a[i * 2]) / 2;
        }
        for (int i = 0; i < 12*32; i++)
        {
            int index = 100 * (i / 32) + 3 * (i % 32) + 4;
            r[i] = BitConverter.ToUInt16(data, index) / 500f;
            intensity[i] = data[index + 2];
            z[i] = r[i] * (float)Math.Cos(w[i % 16]) * (float)Math.Sin(a[i / 16]);
            x[i] = r[i] * (float)Math.Cos(w[i % 16]) * (float)Math.Cos(a[i / 16]);
            y[i] = r[i] * (float)Math.Sin(w[i % 16]);
        }
    }
}
