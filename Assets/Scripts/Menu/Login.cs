using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    [SerializeField]
    Player player;
    public InputField inputFieldEmail;
    public InputField inputFieldContra;
    public GameObject regionLogin;
    public GameObject regionRegistrar;

    private void Start()
    {
        player = FindObjectOfType<Player>();
        inputFieldContra.text = "seCret_20";
    }
    public void ButtonLoginClicked()
    {
        StartCoroutine(TryLogin());
    }

    private IEnumerator GetToken()
    {
        WWWForm data = new WWWForm();

        data.AddField("grant_type", "password");
        data.AddField("username", inputFieldEmail.text);
        data.AddField("password", inputFieldContra.text);

        using (UnityWebRequest httpClient = UnityWebRequest.Post(player.HttpServerAddress + "/Token", data))
        {

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetToken > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                AuthToken authToken = JsonUtility.FromJson<AuthToken>(jsonResponse);
                player.Token = authToken.access_token;
            }
        }
    }
    private IEnumerator GetInfoPlayer()
    {

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Player/GetPlayerInfo/" + player.Id, "GET"))
        {

            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
            httpClient.SetRequestHeader("Accept", "application/json");

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetInfoPlayer > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                PlayerJson playerJson = JsonUtility.FromJson<PlayerJson>(jsonResponse);
                player.FirstName = playerJson.FirstName;
                player.LastName = playerJson.LastName;
                player.DateOfBirth = playerJson.DateOfBirth;
                player.NickName = playerJson.NickName;
                player.Email = playerJson.Email;
                player.City = playerJson.City;
                player.DateJoined = playerJson.DateJoined;
                player.BlobUri = playerJson.BlobUri;
                

            }

        }

    }
    private IEnumerator TryLogin()
    {

        if (string.IsNullOrEmpty(player.Token))
        {
            yield return GetToken();
        }

        using(UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Account/UserId"))
        {
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
            httpClient.SetRequestHeader("Accept", "application/json");

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("TryLogin > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                player.Id = httpClient.downloadHandler.text.Replace("\"","");
            }
        }
        yield return GetInfoPlayer();
        SceneManager.LoadScene("Menu Partidas");
    }


    public void BotonRegistrar()
    {
        regionLogin.SetActive(false);
        regionRegistrar.SetActive(true);
    }


}
