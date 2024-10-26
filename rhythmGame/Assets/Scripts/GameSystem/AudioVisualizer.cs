using UnityEngine;

public class AudioVisualizer : MonoBehaviour
{
    private AudioSource audioSource;
    private float[] samples = new float[512];
    private float[] freqBand = new float[8];
    private float[] bandBuffer = new float[8];
    private float[] bufferDecrease = new float[8];

    public float startScale = 1f;
    public float scaleMultiplier = 1f;
    public float decreaseSpeed = 0.005f;
    public float sensitivity = 100f;

    public GameObject[] visualizerObjects;
    private int[] objectBandAssignment;  // 각 오브젝트가 어떤 주파수 대역에 반응할지 저장

    private float[] bufferRateDecrease = new float[8];
    private float[] freqBandHighest = new float[8];

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // 각 오브젝트에 랜덤하게 주파수 대역 할당
        objectBandAssignment = new int[visualizerObjects.Length];
        for (int i = 0; i < visualizerObjects.Length; i++)
        {
            objectBandAssignment[i] = Random.Range(0, 8);
        }

        // 초기값 설정
        for (int i = 0; i < 8; i++)
        {
            bufferRateDecrease[i] = 0.005f;
            freqBandHighest[i] = 1f;
        }
    }

    void Update()
    {
        if (audioSource == null || !audioSource.isPlaying) return;

        GetSpectrumAudioSource();
        MakeFrequencyBands();
        BandBuffer();
        CreateAudioBands();

        // 각 시각화 오브젝트 업데이트
        for (int i = 0; i < visualizerObjects.Length; i++)
        {
            if (visualizerObjects[i] != null)
            {
                // 할당된 주파수 대역 가져오기
                int bandIndex = objectBandAssignment[i];

                // 현재 스케일 가져오기
                Vector3 currentScale = visualizerObjects[i].transform.localScale;

                // 새로운 Y 스케일 계산
                float scaleY = startScale + (bandBuffer[bandIndex] * scaleMultiplier);

                // 부드러운 전환
                float newScaleY = Mathf.Lerp(currentScale.y, scaleY, Time.deltaTime * 10f);

                // 스케일 적용
                visualizerObjects[i].transform.localScale = new Vector3(
                    currentScale.x,
                    newScaleY,
                    currentScale.z
                );
            }
        }
    }

    void GetSpectrumAudioSource()
    {
        audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
    }

    void BandBuffer()
    {
        for (int i = 0; i < 8; i++)
        {
            if (freqBand[i] > bandBuffer[i])
            {
                bandBuffer[i] = freqBand[i];
                bufferDecrease[i] = decreaseSpeed;
            }

            if (freqBand[i] < bandBuffer[i])
            {
                bandBuffer[i] -= bufferDecrease[i];
                bufferDecrease[i] *= 1.2f;
            }
        }
    }

    void CreateAudioBands()
    {
        for (int i = 0; i < 8; i++)
        {
            if (freqBand[i] > freqBandHighest[i])
            {
                freqBandHighest[i] = freqBand[i];
            }

            float normalizedValue = (freqBand[i] / freqBandHighest[i]) * sensitivity;
            freqBand[i] = Mathf.Clamp01(normalizedValue);
        }
    }

    void MakeFrequencyBands()
    {
        int count = 0;

        for (int i = 0; i < 8; i++)
        {
            float average = 0;
            int sampleCount = (int)Mathf.Pow(2, i + 1);

            if (i == 7)
            {
                sampleCount += 2;
            }

            for (int j = 0; j < sampleCount; j++)
            {
                if (count < samples.Length)
                {
                    average += samples[count] * (count + 1);
                    count++;
                }
            }

            average /= count;
            freqBand[i] = average * 10;
        }
    }

    // 디버깅용: 각 오브젝트가 어떤 주파수 대역에 할당되었는지 확인
    public void PrintBandAssignments()
    {
        for (int i = 0; i < visualizerObjects.Length; i++)
        {
            Debug.Log($"Object {i}: Band {objectBandAssignment[i]}");
        }
    }
}