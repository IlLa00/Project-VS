using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Firestore;
using UnityEngine;

namespace VS.Core
{
    public class FirebaseManager : MonoBehaviour
    {
        public static FirebaseManager Instance { get; private set; }

        public string UserId { get; private set; }
        public bool IsReady { get; private set; }
        public FirebaseFirestore Db { get; private set; }
        public FirebaseDatabase Rtdb { get; private set; }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Initialize()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(depTask =>
            {
                if (depTask.Result != DependencyStatus.Available)
                {
                    Debug.LogWarning($"[Firebase] dependency failed: {depTask.Result}");
                    return;
                }

                FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith(authTask =>
                {
                    if (authTask.IsFaulted || authTask.IsCanceled)
                    {
                        Debug.LogWarning("[Firebase] anonymous sign-in failed");
                        return;
                    }
                    UserId = authTask.Result.User.UserId;
                    Db = FirebaseFirestore.DefaultInstance;
                    Rtdb = FirebaseDatabase.DefaultInstance;
                    IsReady = true;
                    Debug.Log($"[Firebase] Ready. UID={UserId}");
                });
            });
        }
    }
}
