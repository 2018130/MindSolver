using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/**
 * @brief 효과음 데이터를 저장하는 직렬화 클래스입니다.
 */
[System.Serializable]
public class SFXData
{
    public string name;      // 소리의 이름 (예: "Win", "Lose")
    public AudioClip clip;   // 실제 소리 파일
}

/**
 * @brief 전역 사운드 재생을 관리하는 싱글톤 매니저 클래스입니다.
 */
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Mixer & Source")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    [Header("Default Touch Sound")]
    [SerializeField] private AudioClip touchClip; // 항상 유지되는 터치음!

    [Header("SFX Library")]
    [SerializeField] private List<SFXData> sfxLibrary = new List<SFXData>(); // 필요할 때 꺼내 쓸 추가 효과음들!

    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitSFXDictionary(); // 시작할 때 효과음 목록 정리하기!
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /**
     * @brief 리스트에 있는 데이터를 이름으로 찾기 쉽게 사전으로 옮깁니다.
     */
    private void InitSFXDictionary()
    {
        foreach (var data in sfxLibrary)
        {
            if (!sfxDictionary.ContainsKey(data.name))
            {
                sfxDictionary.Add(data.name, data.clip);
            }
        }
    }

    private void OnEnable()
    {
        //SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        //SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 열릴 때마다 모든 버튼에 터치음을 몰래 추가하기!
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button btn in allButtons)
        {
            btn.onClick.AddListener(PlayTouchSound);
        }
    }

    /**
     * @brief 고정된 터치 효과음을 약간의 피치 변동과 함께 즉시 재생합니다.
     */
    public void PlayTouchSound()
    {
        if (touchClip != null)
        {
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(touchClip);
        }
    }

    /**
     * @brief 이름(string)을 입력받아 추가 등록된 효과음을 재생합니다.
     * @param sfxName 등록된 효과음의 이름
     */
    public void PlaySFX(string sfxName)
    {
        if (sfxDictionary.TryGetValue(sfxName, out AudioClip clip))
        {
            // 다른 효과음은 피치 조절 없이 원래 소리로 재생!
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"'{sfxName}'이라는 이름의 소리는 등록되지 않았어! 확인해봐!");
        }
    }
    public void StopAllSFX()
    {
        sfxSource.Stop();
    }

    public void PlayBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.Play();
    }
}