using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSynthesizer : MonoBehaviour
{
    public float frequency = 440;
    private float increment;
    private float phase;
    private float sampling = 48000;

    public float gain;

    private void OnAudioFilterRead(float[] data, int channels)
    {
        increment = frequency * 2 * Mathf.PI / sampling;

        for (int i = 0; i < data.Length; i += channels)
        {
            phase += increment;
            data[i] = gain * Mathf.Sin(phase);

            if (channels == 2)
            {
                data[i + 1] = data[i];
            }

            if (phase > Mathf.PI * 2)
            {
                phase = 0;
            }
        }
    }
}
