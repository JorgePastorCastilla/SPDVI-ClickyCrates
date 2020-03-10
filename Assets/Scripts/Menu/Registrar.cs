using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Registrar : MonoBehaviour
{
    [SerializeField]
    Player player;
    public InputField inputFieldEmail;
    public InputField inputFieldContra;
    public InputField inputFieldConfirmarContra;
    public InputField inputFieldNombre;
    public InputField inputFieldApellidos;
    public InputField inputFieldFechaDeNacimiento;
    public InputField inputFieldApodo;
    public InputField inputFieldCiudad;
    public GameObject regionLogin;
    public GameObject regionRegistrar;

    private Image pruebaImagen;

    private void Start()
    {
        player = FindObjectOfType<Player>();
        inputFieldContra.text = "seCret_20";
        inputFieldConfirmarContra.text = "seCret_20";
        inputFieldNombre.text = "Pepito";
        inputFieldApellidos.text = "Martinez Perez";
        inputFieldApodo.text = "Pepi";
        inputFieldFechaDeNacimiento.text = "1995-12-29";
        inputFieldCiudad.text = "Inca";
    }

    public void BotonRegistrar()
    {
        StartCoroutine(RegisterNewPlayer());
    }
    private IEnumerator RegisterNewPlayer()
    {
        // Comprobamos que la contraseña sea igual que el confirmar contraseña
        if(inputFieldContra.text == inputFieldConfirmarContra.text)
        {
            // Comprobamos que hay información en el campo del email
            if (!string.IsNullOrEmpty(inputFieldEmail.text))
            {
                yield return RegistrarAspNetUser();
                yield return GetAuthenticationToken();
                yield return GetAspNetUserId();
                yield return InsertPlayer();
                //yield return LoadImage();
            }
        }

    }

    private IEnumerator RegistrarAspNetUser()
    {
        
        AspNetUserModel aspUser = new AspNetUserModel();

        aspUser.Email = inputFieldEmail.text;
        aspUser.Password = inputFieldContra.text;
        aspUser.ConfirmPassword = inputFieldConfirmarContra.text;

        using(UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Account/Register", "POST"))
        {
            
            string bodyJson = JsonUtility.ToJson(aspUser);
            
            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.SetRequestHeader("Content-type", "application/json");

            yield return httpClient.SendWebRequest();

            if(httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("RegistrarAspNetUser > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
        }


    }
    private IEnumerator GetAuthenticationToken()
    {

        WWWForm data = new WWWForm();

        data.AddField("grant_type", "password");
        data.AddField("username", inputFieldEmail.text);
        data.AddField("password", inputFieldContra.text);

        using(UnityWebRequest httpClient = UnityWebRequest.Post(player.HttpServerAddress + "/Token", data))
        {

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetAuthenticationToken > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                AuthToken authToken = JsonUtility.FromJson<AuthToken>(jsonResponse);
                player.Token = authToken.access_token;
            }
        }
    }
    private IEnumerator GetAspNetUserId()
    {

        using(UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Account/UserId", "GET"))
        {

            byte[] bodyRaw = Encoding.UTF8.GetBytes("Nothing");

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Accept", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetAspNetUserId > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                player.Id = httpClient.downloadHandler.text.Replace("\"", "");
            }

        }

    }

    private IEnumerator InsertPlayer()
    {
        PlayerSerializable playerJson = new PlayerSerializable();
        playerJson.Id = player.Id;
        playerJson.Email = inputFieldEmail.text;
        playerJson.FirstName = inputFieldNombre.text;
        playerJson.LastName = inputFieldApellidos.text;
        playerJson.NickName = inputFieldApodo.text;
        playerJson.City = inputFieldCiudad.text;
        playerJson.DateOfBirth = inputFieldFechaDeNacimiento.text;
        //playerJson.BlobUri = "https://spdvistoragemrp.blob.core.windows.net/clickycrates-blobs/default/001.png";
        //player.BlobUri = playerJson.BlobUri;
        playerJson.BlobUri = "https://yt3.ggpht.com/a/AGF-l7_4Kpno-60TGCBVa91UqlaQ57xt_I7s0OR1=s900-c-k-c0xffffffff-no-rj-mo";

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Player/InsertNewPlayer", "POST"))
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
                throw new System.Exception("InsertPlayer > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                regionLogin.SetActive(true);
                regionRegistrar.SetActive(false);
                
            }

        }
    }

    


    public void BotonAtras()
    {
        regionRegistrar.SetActive(false);
        regionLogin.SetActive(true);
    }

}
