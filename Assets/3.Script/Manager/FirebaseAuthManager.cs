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

        PlayGamesPlatform.Activate();

        SignInWithGoogle();
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
        Debug.Log("[GPGS] 로그인 시도 시작...");

        PlayGamesPlatform.Instance.Authenticate((status) =>
        {
            // 1. 로그인 결과 상태 출력
            Debug.Log($"[GPGS] Authenticate 결과 상태: {status}");

            if (status == SignInStatus.Success)
            {
                Debug.Log("[GPGS] 로그인 성공! Firebase 연동을 위한 Server Auth Code 요청 중...");

                // 2. 서버 인증 코드 요청
                // 첫 번째 인자인 forceRefresh를 true로 설정하면 새로운 코드를 강제로 가져옵니다.
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, (authCode) =>
                {
                    if (string.IsNullOrEmpty(authCode))
                    {
                        Debug.LogError("[GPGS] 서버 인증 코드 획득 실패: authCode가 null이거나 비어있습니다. (설정/인증서 문제 가능성)");
                    }
                    else
                    {
                        Debug.Log($"[GPGS] 서버 인증 코드 획득 완료: {authCode}");
                        SignInFromGoogle(authCode);
                    }
                });
            }
            else
            {
                // 3. 실패 시 상세 케이스 분류
                string reason = "알 수 없는 이유";
                switch (status)
                {
                    case SignInStatus.Canceled:
                        reason = "사용자가 로그인을 취소했거나, 인증서(SHA-1) 불일치로 시스템이 취소함.";
                        break;
                    case SignInStatus.InternalError:
                        reason = "구글 서비스 내부 오류. 네트워크 상태나 Google Play 서비스 앱 업데이트 확인 필요.";
                        break;
                    default:
                        reason = $"기타 에러 코드: {status}";
                        break;
                }

                Debug.LogError($"[GPGS] 로그인 최종 실패 원인: {reason}");

                // 팁: 여기서 구글 플레이 서비스 앱이 최신인지 확인하는 로직을 추가할 수도 있습니다.
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
