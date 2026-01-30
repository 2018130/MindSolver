using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

public class FirebaseAuthManager : SingletonBehaviour<FirebaseAuthManager>
{
    private FirebaseAuth _auth;
    private FirebaseUser _user;
    public bool IsLogIned => _user != null;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                _auth = FirebaseAuth.DefaultInstance;
                TitleSceneUIManager.Singleton.SetActiveAuthButtons(true);
                Debug.Log("Firebase dependencies resolved and initialized.");
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });

    }

    public void SignUp(string id, string pwd)
    {
        _auth.CreateUserWithEmailAndPasswordAsync(id, pwd).ContinueWith((task) =>
        {
            if (task.IsCanceled)
            {
                Debug.LogWarning($"Sign up task was canceled.");
                return;
            }

            if(task.IsFaulted)
            {
                Debug.LogWarning($"Failed to sign up, reason : {task.Exception}");
                return;
            }

            Debug.Log($"Sign up successful!!!");
        });
    }
    
    public void SignIn(string id, string pwd)
    {
        Debug.Log($"[Auth] Attempting Sign-in with: {id}");

        _auth.SignInWithEmailAndPasswordAsync(id, pwd).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogWarning("Sign-in canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogWarning("Sign-in faulted.");
                return;
            }

            _user = task.Result.User;
            Debug.Log($"Sign in successful!!! name : {_user.ProviderId}");
        });
    }

    public void SignInWithGoogle()
    {
        PlayGamesPlatform.Instance.Authenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("GPGS 로그인 성공! Firebase 연동 시작...");

                // 3. 서버 인증 코드 요청 (Firebase 연동 핵심)
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, (authCode) =>
                {
                    Debug.Log($"서버 인증 코드 획득: {authCode}");
                    SignInFromGoogle(authCode);
                });
            }
            else
            {
                Debug.LogError($"GPGS 로그인 실패: {status}");
            }
        });
    }
    private void SignInFromGoogle(string authCode)
    {
        // 4. GPGS 전용 인증 정보(Credential) 생성
        Credential credential = PlayGamesAuthProvider.GetCredential(authCode);

        // 5. Firebase 로그인 수행
        _auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Firebase 로그인 취소");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"Firebase 로그인 오류: {task.Exception}");
                return;
            }

            // 로그인 성공
            _user = task.Result.User;
            Debug.LogFormat("Firebase 연동 성공: {0} ({1})", _user.DisplayName, _user.UserId);
        });
    }
    public void SignOut()
    {
        if (_user != null)
        {
            _user = null;
            _auth.SignOut();
        }
    }
}
