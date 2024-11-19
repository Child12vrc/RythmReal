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
    private int[] objectBandAssignment;

    private float[] bufferRateDecrease = new float[8];
    private float[] freqBandHighest = new float[8];

    private bool isInitialized = false;

    void Start()
    {
        InitializeVisualizer();
    }

    public void InitializeVisualizer()
    {
        // AudioSource 가져오기
        audioSource = GetComponent<AudioSource>();

        if (!isInitialized && visualizerObjects != null && visualizerObjects.Length > 0)
        {
            // 각 오브젝트에 랜덤하게 주파수 대역 할당
            objectBandAssignment = new int[visualizerObjects.Length];
            for (int i = 0; i < visualizerObjects.Length; i++)
            {
                objectBandAssignment[i] = Random.Range(0, 8);

                // 초기 스케일 설정
                if (visualizerObjects[i] != null)
                {
                    visualizerObjects[i].transform.localScale = new Vector3(
                        visualizerObjects[i].transform.localScale.x,
                        startScale,
                        visualizerObjects[i].transform.localScale.z
                    );
                }
            }

            // 초기값 설정
            for (int i = 0; i < 8; i++)
            {
                bufferRateDecrease[i] = 0.005f;
                freqBandHighest[i] = 1f;
                bandBuffer[i] = 0f;
                bufferDecrease[i] = decreaseSpeed;
            }

            isInitialized = true;
        }
    }

    void Update()
    {
        // AudioSource 유효성 검사 강화
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) return;
        }

        if (!audioSource.isPlaying)
        {
            ResetVisualizers();
            return;
        }

        GetSpectrumAudioSource();
        MakeFrequencyBands();
        BandBuffer();
        CreateAudioBands();
        UpdateVisualizers();
    }

    private void UpdateVisualizers()
    {
        if (visualizerObjects == null) return;

        for (int i = 0; i < visualizerObjects.Length; i++)
        {
            if (visualizerObjects[i] != null)
            {
                int bandIndex = objectBandAssignment[i];
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

    private void ResetVisualizers()
    {
        if (visualizerObjects == null) return;

        for (int i = 0; i < visualizerObjects.Length; i++)
        {
            if (visualizerObjects[i] != null)
            {
                Vector3 currentScale = visualizerObjects[i].transform.localScale;
                visualizerObjects[i].transform.localScale = new Vector3(
                    currentScale.x,
                    startScale,
                    currentScale.z
                );
            }
        }
    }

    public void OnAudioSourceChanged()
    {
        // AudioSource 다시 가져오기
        audioSource = GetComponent<AudioSource>();

        // 시각화 초기화
        if (!isInitialized)
        {
            InitializeVisualizer();
        }
        else
        {
            // 기존 값들 초기화
            for (int i = 0; i < 8; i++)
            {
                freqBand[i] = 0f;
                bandBuffer[i] = 0f;
                bufferDecrease[i] = decreaseSpeed;
                freqBandHighest[i] = 1f;
                bufferRateDecrease[i] = 0.005f;
            }
        }

        // 시각화 오브젝트 초기 상태로
        if (visualizerObjects != null)
        {
            foreach (var obj in visualizerObjects)
            {
                if (obj != null)
                {
                    obj.transform.localScale = new Vector3(
                        obj.transform.localScale.x,
                        startScale,
                        obj.transform.localScale.z
                    );
                }
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

    void OnDisable()
    {
        ResetVisualizers();
    }

    // 디버깅용
    public void PrintBandAssignments()
    {
        for (int i = 0; i < visualizerObjects.Length; i++)
        {
            Debug.Log($"Object {i}: Band {objectBandAssignment[i]}");
        }
    }
}