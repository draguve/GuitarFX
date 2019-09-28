using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using B83.MathHelpers;
using Lasp;
using ProceduralToolkit;


public class AudioInput : MonoBehaviour
{
    public ParticleSystem objectToColorChange;
    public int size = 4096; // more than 1024
    public int sampFreq = 22050;
    public int minNote =  40;
    public int maxNote = 95;
    public float noteThreshold = 0f;
    
    Complex[] freq;
    private float[] samples= new float[1024];
    private float[] lastsamples;
    private int fakeSize;
    private float freqStep;
    private string[] NOTES;
    private ColorHSV[] Colors;
    private int imin, imax;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
        freq = new Complex[size];
        fakeSize = size - 1024;
        lastsamples = new float[fakeSize];
        freqStep = (float)sampFreq / (size/2f);
        NOTES = "C C# D D# E F F# G G# A A# B".Split(' ');
        Colors = new ColorHSV[12];
        Colors[0] = new ColorHSV(Color.red);
        for (int i = 1; i < 12; i++)
        {
            Colors[i] = Colors[0].WithH(i*(360f/48f));
            Debug.Log(Colors[i].ToString());
        }
        imin = (int)Mathf.Max(0, Mathf.Floor(note_to_fftbin(minNote - 1)));
        imax = (int)Mathf.Min(size/2, (Mathf.Ceil(note_to_fftbin(maxNote + 1))));

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

        List<FreqData> maximas = new List<FreqData>();
        
        for (int i = imin; i < imax; i++)
        {
            if (freq[i].magnitude > noteThreshold)
            {
                if (freq[i].magnitude > freq[i - 1].magnitude && freq[i].magnitude > freq[i + 1].magnitude)
                {
                    FreqData temp = new FreqData();
                    temp.location = i;
                    temp.mag = freq[i].magnitude;
                    maximas.Add(temp);
                }
            }
        }

        if (maximas.Count > 0)
        {
            maximas.Sort((s1, s2) => s1.mag.CompareTo(s2.mag));
            float maxFreq = maximas[0].location * freqStep;
            int closest = (int) FreqToNumber(maxFreq);
            float closestFreq = NumberToFreq(closest);
            ColorHSV colorA = Colors[closest % 12],colorB;
            if (maxFreq - closestFreq > 0)
            {
                colorB = Colors[(closest + 1) % 12];
            }
            else
            {
                colorB = Colors[(closest - 1) % 12];
            }

            var mainModule = objectToColorChange.main;
            Color x =  ColorHSV.Lerp(colorA, colorB, Mathf.Abs(maxFreq - closestFreq) / maxFreq).ToColor();
            mainModule.startColor = x;
        }
    }

    struct FreqData
    {
        public int location;
        public double mag;
    }


    float FreqToNumber(float f)
    {
        return 69 + 12 * Mathf.Log(f / 440.0f,2);
    }

    float NumberToFreq(int n)
    {
        return 440f * Mathf.Pow(2, (n - 69f) / 12.0f);
    }

    string noteName(int n)
    {
        return NOTES[n % 12] + (n / 12 - 1);
    }

    int note_to_fftbin(int n)
    {
        return (int) (NumberToFreq(n) / freqStep);
    }
}
