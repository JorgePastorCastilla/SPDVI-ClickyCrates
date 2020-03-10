using Assets.Scripts.Modelos;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DifficultyButton : MonoBehaviour
{
    private Button button;
    private GameManager gameManager;
    public int difficulty;
    public string[] difficultyText = { "Easy", "Medium", "Hard" };
    private Player player;
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SetDifficulty);
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        player = FindObjectOfType<Player>();
    }

    public void SetDifficulty()
    {
        StartCoroutine( InsertGame() );
        //gameManager.StartGame(difficulty);
    }
    private IEnumerator InsertGame()
    {
        GameSerializable game = new GameSerializable();
        game.PlayerId = player.Id;
        game.Difficulty = difficultyText[difficulty - 1];

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Game/InsertNewGame", "POST"))
        {
            string playerData = JsonUtility.ToJson(game);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);
            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("InsertNewGame > Error: " + httpClient.responseCode + ", Info: " + httpClient.error );
            }
            else
            {
                gameManager.StartGame(difficulty);
            }
        }   
    }
}
