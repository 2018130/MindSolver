using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 

/**
 * @brief 전역 사운드 재생을 관리하는 싱글톤 매니저 클래스입니다.
 * * 씬 전환 시에도 파괴되지 않으며, 배경음 및 효과음 재생을 담당합니다.
 */
public class SoundManager : MonoBehaviour
{
    /** @brief 싱글톤 인스턴스 */
    public static SoundManager Instance { get; private set; }

    /** @brief 오디오 믹서 참조 */
    [SerializeField] private AudioMixer audioMixer;

    /** @brief 효과음 전용 오디오 소스 */
    [SerializeField] private AudioSource sfxSource;
   
    /** @brief 터치 효과음으로 사용할 오디오 클립 */
    [SerializeField] private AudioClip touchClip;

    /**
     * @brief 인스턴스를 초기화하고 중복 생성을 방지합니다.
     */
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /**
     * @brief 효과음을 재생합니다.
     * @param clip 재생할 오디오 클립
     */
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    /**
     * @brief 설정된 터치 효과음을 즉시 재생합니다.
     */
    public void PlayTouchSound()
    {
        if (touchClip != null)
        {
            // 소리의 높낮이를 0.95에서 1.05 사이로 랜덤하게 설정! 
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(touchClip);
        }
    }

    private void OnEnable()
    {
        // 씬이 로드될 때마다 'OnSceneLoaded' 함수를 실행해줘! 라고 예약하는 거야 
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 오브젝트가 사라질 땐 예약을 취소하는 매너! 
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새로운 씬이 열리면 그 씬에 있는 모든 버튼을 다 찾아버리자! 
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();

        foreach (Button btn in allButtons)
        {
            // 버튼을 눌렀을 때 우리 터치 소리가 나도록 몰래 추가해두기! 
            btn.onClick.AddListener(PlayTouchSound);
        }

    }

}