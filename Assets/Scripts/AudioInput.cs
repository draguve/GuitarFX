using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using B83.MathHelpers;
using Lasp;


public class AudioInput : MonoBehaviour
{
    
    Complex[] freq = new Complex[2048];
    private float[] samples = new float[1024];
    private float[] lastsamples = new float[1024];
    
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < lastsamples.Length; i++)
        {
            lastsamples[i] = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < lastsamples.Length; i++)
        {
            lastsamples[i] = samples[i];
        }
        
        MasterInput.RetrieveWaveform(FilterType.Bypass,samples);
        
        for(int i = 0; i < lastsamples.Length; i++)
        {
            freq[i] = new Complex(lastsamples[i],0);
        }
        for(int i = 0; i < samples.Length; i++)
        {
            freq[1024 + i] = new Complex(samples[i],0);
        }
        
        for (int i = 0; i < samples.Length; i++)
        {
            Debug.DrawLine(new Vector3(i, 0), new Vector3(i, samples[i] * 20), Color.cyan);
        }
        
        FFT.CalculateFFT(freq, false);   
        
        for (int i = 0; i < freq.Length/2; i++) // plot only the first half
        {
            // multiply the magnitude of each value by 2
            Debug.DrawLine(new Vector3(i, 4), new Vector3(i, 20+(float)freq[i].magnitude * 200), Color.white);
        }
    }
}
