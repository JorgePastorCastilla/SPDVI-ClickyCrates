using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Perfil : MonoBehaviour
{
    [SerializeField]
    GameObject playerGO;
    private Player player;
    public InputField inputFieldNombre;
    public InputField inputFieldApellidos;
    public InputField inputFieldApodo;
    public InputField inputFieldEmail;
    public InputField inputFieldBlobUri;
    public Button buttonEditar;
    public Image imagenAvatar;
    private bool estaEditando = false;

    void Start()
    {
        playerGO = GameObject.Find("Player");
        player = playerGO.GetComponent<Player>();
        StartCoroutine(LoadImage());
        GetInfoPlayer();
    }

    public void GoToPartidasButton()
    {
        SceneManager.LoadScene("Menu Partidas");
    }

    private void GetInfoPlayer()
    {
        inputFieldNombre.text = player.FirstName;
        inputFieldApellidos.text = player.LastName;
        inputFieldApodo.text = player.NickName;
        inputFieldEmail.text = player.Email;
        inputFieldBlobUri.text = player.BlobUri;
    }

    private IEnumerator LoadImage()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(player.BlobUri))
        {
            httpClient.downloadHandler = new DownloadHandlerTexture();
            yield return httpClient.SendWebRequest();
            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("LoadImage > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(httpClient);
                imagenAvatar.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
        }
    }

    public void EditarModoOnButton()
    {
        if (estaEditando)
        {
            inputFieldNombre.interactable = false;
            inputFieldApellidos.interactable = false;
            inputFieldApodo.interactable = false;
            inputFieldEmail.interactable = false;
            inputFieldBlobUri.interactable = false;
            estaEditando = false;
            StartCoroutine(UpdateInfoPlayer());
        }
        else
        {
            inputFieldNombre.interactable = true;
            inputFieldApellidos.interactable = true;
            inputFieldApodo.interactable = true;
            inputFieldEmail.interactable = true;
            inputFieldBlobUri.interactable = true;
            estaEditando = true;
        }
    }
    private IEnumerator UpdateInfoPlayer()
    {
        PlayerSerializable playerJson = new PlayerSerializable();
        playerJson.Id = player.Id;
        playerJson.FirstName = inputFieldNombre.text;
        playerJson.LastName = inputFieldApellidos.text;
        playerJson.NickName = inputFieldApodo.text;

        using(UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Player/UpdatePlayer", "POST"))
        {
            string playerData = JsonUtility.ToJson(playerJson);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);
            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);
            httpClient.downloadHandler = new DownloadHandlerBuffer();
            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
            yield return httpClient.SendWebRequest();
            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("UpdateInfoPlayer > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                player.FirstName = playerJson.FirstName;
                player.LastName = playerJson.LastName;
                player.NickName = playerJson.NickName;
                //yield return UpdateInfoOnline();
            }
        }

    }


    private IEnumerator UpdateInfoOnline()
    {
        OnlineSerializable online = new OnlineSerializable();
        online.Id = player.Id;
        online.Name = player.NickName;

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Online/UpdateOnline", "POST"))
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
                throw new System.Exception("UpdateInfoOnline > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
        }
    }


    public void EliminarCuentaButton()
    {
        StartCoroutine(EliminarCuenta());
    }

    private IEnumerator EliminarCuenta()
    {
        yield return EndSession();
        PlayerSerializable playerJson = new PlayerSerializable();
        playerJson.Id = player.Id;
       
        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Account/DeleteAccount", "POST"))
        {
            string playerData = JsonUtility.ToJson(playerJson);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("EliminarCuenta > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
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

                SceneManager.LoadScene("Menu");
                
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
