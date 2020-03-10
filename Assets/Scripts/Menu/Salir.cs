using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Text;

public class Salir : MonoBehaviour
{

    [SerializeField]
    Player player;

    private void Start()
    {
        player = FindObjectOfType<Player>();
    }

    public void SalirAplicacion()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
        Application.Quit();
#elif UNITY_ANDROID
        Application.Quit();
#endif
    }

    public void SalirConLogOut()
    {
        StartCoroutine(Cerrar());
    }


    public void SalirDefinitivo()
    {
        StartCoroutine(CerrarDefinitivo());
    }

    public IEnumerator CerrarDefinitivo()
    {
        yield return EndSession();
        yield return TrySalir();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
        Application.Quit();
#elif UNITY_ANDROID
        Application.Quit();
#endif
    }

    public IEnumerator Cerrar()
    {
        yield return EndSession();
        yield return TrySalir();
        SceneManager.LoadScene("Menu");
    }
    private IEnumerator TrySalir()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Account/Logout","POST"))
        {
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("TrySalir > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                player.Id = string.Empty;
                player.Token = string.Empty;
                player.FirstName = string.Empty;
                player.LastName = string.Empty;
                player.Email = string.Empty;
                player.NickName = string.Empty;
                player.BlobUri = string.Empty;
                player.City = string.Empty;            
            }

        }

        
    }


    private IEnumerator EndSession()
    {
        OnlineSerializable online = new OnlineSerializable();
        online.Id = player.Id;

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/player/logout", "POST"))
        {
            string playerData = JsonUtility.ToJson(online);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);
            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);
            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("EndSession > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                player.Login = false;

            }

        }

    }

}
