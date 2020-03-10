using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControlDatos : MonoBehaviour
{

    [SerializeField]
    Player player;
    public Text nickNameText;
    public Image imagenAvatar;
    public GameObject OnlinePlayersContainer;
    public GameObject playerOnline;
    void Start()
    {
        player = FindObjectOfType<Player>();
        GetNickName();
        StartCoroutine(LoadImage());
        StartCoroutine(GetAllPlayers());
    }

    
    private void GetNickName()
    {
        nickNameText.text = player.NickName;
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

    public void GoMenuJugadorButton()
    {
        SceneManager.LoadScene("Menu Partidas");
    }
    private IEnumerator GetAllPlayers()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/player/OnlinePlayers", "GET"))
        {
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
            httpClient.SetRequestHeader("Accept", "application/json");
            httpClient.downloadHandler = new DownloadHandlerBuffer();
            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetAllPlayers > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                string response = "{\"mylist\":" + jsonResponse + "}";
                ListOnlineSerializable list = JsonUtility.FromJson<ListOnlineSerializable>(response);
                int longitud = list.mylist.Count();
                if (longitud == 1)
                {
                    playerOnline.SetActive(false);
                }
                else
                {
                    foreach (OnlineSerializable o in list.mylist)
                    {
                        if (o.Id == player.Id)
                        {
                            playerOnline.SetActive(false);
                        }
                        else
                        {
                            GameObject newPlayer = Instantiate(playerOnline, Vector3.zero, Quaternion.identity);
                            Image newPlayerImg = newPlayer.transform.GetChild(0).GetComponent<Image>();
                            yield return LoadImageJugadorOnline(newPlayerImg, o.BlobUri);
                            Text newPlayerName = newPlayer.transform.GetChild(1).GetComponent<Text>();
                            newPlayerName.text = o.Name;
                            Text newPlayerMode = newPlayer.transform.GetChild(2).GetComponent<Text>();
                            newPlayerMode.text = o.Mode;
                            Text newPlayerDateLogin = newPlayer.transform.GetChild(3).GetComponent<Text>();
                            newPlayerDateLogin.text = o.DateLogin;
                            newPlayer.transform.SetParent(OnlinePlayersContainer.transform);
                            newPlayer.SetActive(true);
                        }
                    }
                }
            }
        }
    }
    private IEnumerator Refresh()
    {
        while (true)
        {
            DelChildren();
            yield return GetAllPlayers();
            yield return new WaitForSeconds(5f);
        }
    }

    private void DelChildren()
    {
        int children = OnlinePlayersContainer.transform.childCount;
        GameObject child;
        for (int i = 0; i < children; i++)
        {
            child = OnlinePlayersContainer.transform.GetChild(i).gameObject;
            Destroy(child);
        }
    }

    private IEnumerator LoadImageJugadorOnline(Image imagen, string url)
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(url))
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
                imagen.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
        }
    }

}
