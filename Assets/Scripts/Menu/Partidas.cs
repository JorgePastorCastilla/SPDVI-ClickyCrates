using Assets.Scripts.Modelos;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Partidas : MonoBehaviour
{

    [SerializeField]
    Player player;
    public Text textInfoJugador;
    public GameObject partidas;
    public UnityEngine.UI.Image imagenAvatar;
    public Text ultimasPartidas;

    void Start()
    {
        player = FindObjectOfType<Player>();
        partidas = GameObject.Find("Partidas");
        InfoPlayer();
        StartCoroutine(LoadImage());
        StartCoroutine( GetPartidasJugadas() );
        StartCoroutine(InsertOnlinePlayer());
    }


    public void GoToJugarButton()
    {
        SceneManager.LoadScene("Prototype 5");
    }


    public void GoToPerfilButton()
    {
        SceneManager.LoadScene("Perfil");
    }


    private void InfoPlayer()
    {
        textInfoJugador.text = player.NickName;
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

    //TERMINAR ESTO
    private IEnumerator GetPartidasJugadas()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/game/gamelist/" + player.Id, "GET"))
        {

            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
            httpClient.SetRequestHeader("Accept", "application/json");

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetPartidasJugadas > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                string response = "{\"mylist\":" + jsonResponse + "}";
                ListGameSerializable lista = JsonUtility.FromJson<ListGameSerializable>(response);
                foreach (GameSerializable p in lista.mylist)
                {
                    partidas.GetComponent<Text>().text += $"Start: {p.Start}, End: {p.Final}, Difficulty: {p.Difficulty}, Points: {p.Points}\n";
                }

            }
        }
    }


    private IEnumerator IrAlJuego()
    {
        OnlineSerializable online = new OnlineSerializable();
        online.Id = player.Id;

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
            else
            {
                SceneManager.LoadScene("Prototype 5");
            }

        }


    }


    private IEnumerator InsertOnlinePlayer()
    {
        OnlineSerializable online = new OnlineSerializable();
        online.Id = player.Id;
        online.Name = player.NickName;
        online.BlobUri = player.BlobUri;
        online.Mode = "Principiante";

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/player/login", "POST"))
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
                throw new System.Exception("InsertOnlinePlayer > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                player.Login = true;

            }

        }
    }


}
