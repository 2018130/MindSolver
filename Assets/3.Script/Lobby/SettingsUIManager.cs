using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class SettingsUIManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer; // 믹서

    [Header("Sound Control")]
    [SerializeField] private Slider bgmSlider; // BGM 슬라이더
    [SerializeField] private Slider sfxSlider; // 효과음 슬라이더

    [Header("Buttons")]
    [SerializeField] private Button closeButton;        // 닫기 버튼

    [Header("Tutorial Toggle")]
    [SerializeField] private Button checkedButton; // 튜토리얼 완료 상태
    [SerializeField] private Button emptyButton; // 튜토리얼 미완료 상태

    [Space(30f)]
    [SerializeField] private GameObject Settingspanel;
    [SerializeField] private GameObject ResetPanel;
    
    private void Start()
    {

        // 저장된 볼륨 불러오기 (없으면 기본값 1.0)
        bgmSlider.value = PlayerPrefs.GetFloat("BGM_Volume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFX_Volume", 1f);

        // 현재 튜토리얼 상태 확인
        UpdateTutorialUI();

        // 버튼 기능 연결하기
        closeButton.onClick.AddListener(CloseSettings);

        // 튜토리얼 버튼 상태변경
        checkedButton.onClick.AddListener(() => SetTutorialStatus(false));
        emptyButton.onClick.AddListener(() => SetTutorialStatus(true));

        // 슬라이더 연결
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // 소리 크기 적용
        SetBGMVolume(bgmSlider.value);
        SetSFXVolume(sfxSlider.value);

        // 비활성화
        Settingspanel.gameObject.SetActive(false);
        ResetPanel.gameObject.SetActive(false);

        AddSliderPointerUpEvent(sfxSlider);
    }

    /**
     * @brief 슬라이더 조작이 끝났을 때(PointerUp) 이벤트를 추가합니다.
     * @param slider 이벤트를 추가할 UI 슬라이더
     */
    private void AddSliderPointerUpEvent(Slider slider)
    {
        EventTrigger trigger = slider.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = slider.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp; // 손가락을 뗐을 때! 
        entry.callback.AddListener((data) => { PlaySFXPreview(); });
        trigger.triggers.Add(entry);
    }

    /**
     * @brief 효과음 볼륨 확인을 위해 미리보기 소리를 재생합니다.
     */
    public void PlaySFXPreview()
    {
        // 아까 만든 사운드 매니저의 터치 소리를 한 번 빵! 터뜨려줘 
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayTouchSound();
        }
    }

    // BGM 조절
    public void SetBGMVolume(float volume)
    {
        // 슬라이더가 0이면 -80dB(음소거), 아니면 로그
        float db = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20;

        audioMixer.SetFloat("MyBGM", db);
    }

    // SFX 조절
    public void SetSFXVolume(float volume)
    {
        // 슬라이더가 0이면 -80dB(음소거), 아니면 로그
        float db = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20;

        audioMixer.SetFloat("MySFX", db);
    }

    //버튼 상태 바꾸기
    public void SetTutorialStatus(bool isCleared)
    {
        if (isCleared)
        {
            // 완료상태
            PlayerPrefs.SetInt("TutorialClear", 1);
            Debug.Log("튜토리얼 완료처리");
        }
        else
        {
            // 미완료상태
            PlayerPrefs.DeleteKey("TutorialClear");
            Debug.Log("튜토리얼 기록 지우기");
        }

        // 데이터 바꿨으니까 눈에 보이는 버튼도 바로 바꿔주자!
        UpdateTutorialUI();
    }

    private void UpdateTutorialUI()
    {
        // 현재 기록이 있는지 확인 (1이면 완료, 아니면 미완료)
        bool isCleared = PlayerPrefs.GetInt("TutorialClear", 0) == 1;

        if (isCleared)
        {
            // 완료 상태라면 -> 체크된 버튼 보이기, 빈 버튼 숨기기
            checkedButton.gameObject.SetActive(true);
            emptyButton.gameObject.SetActive(false);
        }
        else
        {
            // 미완료 상태라면 -> 빈 버튼 보이기, 체크된 버튼 숨기기
            checkedButton.gameObject.SetActive(false);
            emptyButton.gameObject.SetActive(true);
        }
    }


    // 닫기 버튼
    public void CloseSettings()
    {
        PlayerPrefs.SetFloat("BGM_Volume", bgmSlider.value);
        PlayerPrefs.SetFloat("SFX_Volume", sfxSlider.value);
        PlayerPrefs.Save(); // 저장

        Debug.Log("저장하고 닫기");

        Settingspanel.gameObject.SetActive(false);
    }

    // 튜토리얼 기록 삭제
    public void ResetTutorialData()
    {
        PlayerPrefs.DeleteKey("TutorialClear");
        Debug.Log("튜토리얼 기록 삭제");
    }

    public void Onclicksettingopen()
    {
        Settingspanel.gameObject.SetActive(true);
    }

    public void OnclickResetPanelopen()
    {
        ResetPanel.gameObject.SetActive(true);
    }

    public void CloseResetPanel()
    {
        ResetPanel.gameObject.SetActive(false);
    }

}