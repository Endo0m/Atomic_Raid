using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class InternetConnectionChecker : MonoBehaviour
{
    public static InternetConnectionChecker Instance { get; private set; }

    private const string ConnectivityCheckUrl = "http://www.google.com";

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

    public IEnumerator CheckInternetConnection(System.Action<bool> callback)
    {
        UnityWebRequest request = new UnityWebRequest(ConnectivityCheckUrl);
        yield return request.SendWebRequest();

        bool isConnected = request.result == UnityWebRequest.Result.Success;
        callback(isConnected);
    }
}