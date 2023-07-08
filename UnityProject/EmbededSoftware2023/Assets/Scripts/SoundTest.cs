using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTest : MonoBehaviour
{
    //https://forum.unity.com/threads/generating-a-simple-sinewave.471529/
    [System.Serializable]
    class FrequencyChannel
    {
        //값을 인스펙터에서 바꾸면 click(pop) noise 발생
        public float dstFrequency = 0f;
        public float prevFrequency = 0f;
        public float frequency = 0f;
        public float phase = 0f;
        public float prevValue = 0f;
        public float Amplitude = 1f;
        public void EstimateFreq()
        {
            
            prevFrequency= frequency;
            float step = 100f;
            if(frequency > dstFrequency + step)
            {
                frequency -= step;
            }
            else if(frequency < dstFrequency - step)
            {
                frequency += step;
            }
            else
            {
                frequency = dstFrequency;
            }
        }
    }
    [SerializeField] FrequencyChannel[] freqChannels = new FrequencyChannel[10];

    public float sampleRate = 44100;
    public float waveLengthInSeconds = 2.0f;

    AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0; //force 2D sound
        audioSource.Stop(); //avoids audiosource from starting to play automatically
    }

    void Update()
    {

        freqChannels[0].dstFrequency += 10f * 0.02f;
        if (freqChannels[0].dstFrequency > 100)
        {
            freqChannels[0].dstFrequency = 0;
        }
        freqChannels[1].dstFrequency = freqChannels[0].dstFrequency * 1.5f;
        //freqChannels[2].frequency = freqChannels[0].frequency * 1.5f;
        //freqChannels[3].frequency = freqChannels[0].frequency * 1.333333f;


        if (Input.GetKeyDown(KeyCode.Space))
        {
            

            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        //for (int i = 0; i < data.Length; i += channels)
        //{
        //    data[i] = CreateSine(timeIndex, frequency1, sampleRate);

        //    if (channels == 2)
        //        data[i + 1] = CreateSine(timeIndex, frequency2, sampleRate);

        //    timeIndex++;

        //    //if timeIndex gets too big, reset it to 0
        //    if (timeIndex >= (sampleRate * waveLengthInSeconds))
        //    {
        //        timeIndex = 0;
        //    }
        //}
        //data[i] = CreateSine(timeIndex, frequency1, sampleRate) + CreateSine(timeIndex, frequency2, sampleRate);
        foreach (FrequencyChannel channel in freqChannels)
        {
            channel.EstimateFreq();
        }

        for (int freqIndex = 0; freqIndex < freqChannels.Length; freqIndex ++)
        {
            for (int dataIndex = 0; dataIndex < data.Length; dataIndex += channels)
            {
                float dstValue = Mathf.Sin(freqChannels[freqIndex].phase) * freqChannels[freqIndex].Amplitude;
                //if (Mathf.Abs(dstValue - freqChannels[freqIndex].prevValue) > freqChannels[freqIndex].frequency * 2 * Mathf.PI/sampleRate)//최대기울기는 2pif, 시간단위는 sampleRate이므로 나눠줌
                //{
                //    if(dstValue > freqChannels[freqIndex].prevValue)
                //    {
                //        dstValue = freqChannels[freqIndex].prevValue + freqChannels[freqIndex].frequency * 2 * Mathf.PI / sampleRate;
                //    }
                //    else
                //    {
                //        dstValue = freqChannels[freqIndex].prevValue - freqChannels[freqIndex].frequency * 2 * Mathf.PI / sampleRate;

                //    }
                //}
                data[dataIndex] += dstValue;

                freqChannels[freqIndex].phase += 2 * Mathf.PI * freqChannels[freqIndex].frequency / sampleRate;

                if (freqChannels[freqIndex].phase >= 2 * Mathf.PI)
                {
                    freqChannels[freqIndex].phase -= 2 * Mathf.PI;
                }
                freqChannels[freqIndex].prevValue = dstValue;
            }
            //float tempPhase = freqChannels[freqIndex].phase;
            //for (int dataIndex = 0; dataIndex < data.Length; dataIndex += channels)
            //{
            //    data[dataIndex] += Mathf.Sin(freqChannels[freqIndex].phase) * freqChannels[freqIndex].Amplitude * (1f - ((float)dataIndex) / (float)(data.Length / channels));

            //    freqChannels[freqIndex].phase += 2 * Mathf.PI * freqChannels[freqIndex].prevFrequency / sampleRate;

            //    if (freqChannels[freqIndex].phase >= 2 * Mathf.PI)
            //    {
            //        freqChannels[freqIndex].phase -= 2 * Mathf.PI;
            //    }
            //}
            //freqChannels[freqIndex].phase = tempPhase;
            //for (int dataIndex = 0; dataIndex < data.Length; dataIndex += channels)
            //{
            //    data[dataIndex] += Mathf.Sin(freqChannels[freqIndex].phase) * freqChannels[freqIndex].Amplitude * ((float)dataIndex) / (float)(data.Length / channels);

            //    freqChannels[freqIndex].phase += 2 * Mathf.PI * freqChannels[freqIndex].frequency / sampleRate;

            //    if (freqChannels[freqIndex].phase >= 2 * Mathf.PI)
            //    {
            //        freqChannels[freqIndex].phase -= 2 * Mathf.PI;
            //    }

            //    data[dataIndex] = Mathf.Clamp(data[dataIndex], -1.0f, 1.0f);
            //}
        }
        //if (channels == 2)
        //    data[i + 1] = CreateSine(timeIndex, frequency2, sampleRate);

        //timeIndex++;

        ////if timeIndex gets too big, reset it to 0
        //if (timeIndex >= (sampleRate * waveLengthInSeconds))
        //{
        //    timeIndex = 0;
        //}

    }


}
