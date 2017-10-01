﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateSpheres : MonoBehaviour {
    public long numDataPoints = 1000; // Pick how many data points to display
    public int j = 0;
    private static float maxhex = 255;
    public static GameObject[] dataPointArr; // Declare array of data points
        
	void Start () {
        dataPointArr = new GameObject[numDataPoints]; // Instantiate array

        long i; // Counter
        for (i = 0; i < numDataPoints; i++)
        {
            dataPointArr[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dataPointArr[i].transform.position = new Vector3(0, 0, 0);
        }
	}

    public static void update_sphere(long id, float xpos, float ypos, float zpos, float intensity)
    {
        dataPointArr[id].transform.position = new Vector3(xpos, ypos, zpos);

        Color intensityColor = new Color(0, intensity/maxhex, 0, 1);
        dataPointArr[id].gameObject.GetComponent<Renderer>().material.color = intensityColor;
    }

    void Update () {
        update_sphere(j, j, j, j, j);
        j++;
	}
}