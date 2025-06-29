using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class MultiplayerManager : MonoBehaviourPunCallbacks
{
    public Image LoadingLogo;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        GameManager.Instance.IsMultiplayerMatch = true;
        AnimateLoadingLogo(1);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon. Joining Random Room...");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No room found. Creating a new room...");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room. Waiting for another player...");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("A new player joined the room.");

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Both players are in. Loading game...");
            PhotonNetwork.LoadLevel("Loading");
        }
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Disconnect();
    }

    public  void LeaveRoom()
    {
        GameManager.Instance.ProcessToNextScene("MainMenu");
        PhotonNetwork.LeaveLobby();
    }

    private void AnimateLoadingLogo(float value)
    {
        LoadingLogo.DOFillAmount(value, 1).OnComplete(() =>
       {
           AnimateLoadingLogo(-value);
           LoadingLogo.fillClockwise = !LoadingLogo.fillClockwise;
       });
    }
}
