using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleSceneUIManager : MonoBehaviour
{
    public static TitleSceneUIManager Singleton;

    [SerializeField]
    private Button start_btn;

    [Space(10f)]

    [SerializeField]
    private Button signUp_btn;
    [SerializeField]
    private Button signIn_btn;
    [SerializeField]
    private Button signOut_btn;
    [SerializeField]
    private Button signInWithGoogle_btn;

    [Space(10f)]

    [SerializeField]
    private TMP_InputField id_InputField;
    [SerializeField]
    private TMP_InputField pwd_InputField;

    private void Awake()
    {
        if(Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Singleton.SetActiveButtonAll(false);
    }

    private void Start()
    {
        start_btn.onClick.AddListener(TitleSceneManager.Singleton.StartGame);
        signUp_btn.onClick.AddListener(() => FirebaseAuthManager.Singleton.SignUp(id_InputField.text, pwd_InputField.text));
        signIn_btn.onClick.AddListener(() => FirebaseAuthManager.Singleton.SignIn(id_InputField.text, pwd_InputField.text));
        signInWithGoogle_btn.onClick.AddListener(FirebaseAuthManager.Singleton.SignInWithGoogle);
        signOut_btn.onClick.AddListener(FirebaseAuthManager.Singleton.SignOut);
    }

    private void Update()
    {
        if(FirebaseAuthManager.Singleton.IsLogIned &&
            !start_btn.gameObject.activeSelf)
        {
            SetActiveStartButton(true);
        }
    }

    public void SetActiveButtonAll(bool active)
    {
        SetActiveStartButton(active);
        SetActiveAuthButtons(active);
    }

    public void SetActiveStartButton(bool active)
    {
        start_btn.gameObject.SetActive(active);
    }

    public void SetActiveAuthButtons(bool active)
    {
        signIn_btn.gameObject.SetActive(active);
        signOut_btn.gameObject.SetActive(active);
        signUp_btn.gameObject.SetActive(active);
    }
}
