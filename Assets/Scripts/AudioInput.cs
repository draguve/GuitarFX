﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using B83.MathHelpers;
using Lasp;


public class AudioInput : MonoBehaviour
{
    public int size = 4096; // more than 1024
    public int sampFreq = 22050;
    
    
    Complex[] freq;
    private float[] samples= new float[1024];
    private float[] lastsamples;
    private int fakeSize;
    private float freqStep;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
        freq = new Complex[size];
        fakeSize = size - 1024;
        lastsamples = new float[fakeSize];
        freqStep = sampFreq / (size/2);
    }

    // Update is called once per frame
    void Update()
    {

        for (int i = 1024,j=0; i < fakeSize; i++,j++)
        {
            lastsamples[j] = lastsamples[i];
        }
        for (int i=0,j=fakeSize-1024;j<fakeSize; i++,j++)
        {
            lastsamples[j] = samples[i];
        }


        MasterInput.RetrieveWaveform(FilterType.Bypass,samples);
        
        for(int i = 0; i < lastsamples.Length; i++)
        {
            freq[i] = new Complex(lastsamples[i],0);
        }
        for(int i = 0; i < samples.Length; i++)
        {
            freq[lastsamples.Length + i] = new Complex(samples[i],0);
        }
        
        for (int i = 0; i < samples.Length; i++)
        {
            Debug.DrawLine(new Vector3(i, 0), new Vector3(i, samples[i] * 20), Color.cyan);
        }
        
        FFT.CalculateFFT(freq, false);   
        
        for (int i = 0; i < freq.Length/2; i++) // plot only the first half
        {
            // multiply the magnitude of each value by 2
            Debug.DrawLine(new Vector3(i, 200), new Vector3(i,  200  + (float)freq[i].magnitude * 200), Color.white);
        }
    }
}
