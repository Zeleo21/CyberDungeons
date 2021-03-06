using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Photon
{
    public class NetworkConnection : MonoBehaviourPunCallbacks
    {
        public static bool connected;
        
        [SerializeField] private Text username;
        private string gameVersion = "1";

        public void Connect()
        {
            Debug.Log("Connecting to server...");
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.NickName = username.text;
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to server !");

            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
                connected = true;
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("Deconnecting: " + cause);
        }
    }
}