using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerManager : MonoBehaviour
{
    private List<PlayerInput> players = new List<PlayerInput>();
    [SerializeField] List<LayerMask> playerLayers;

    private PlayerInputManager playerInputManager;
   
    private void OnAwake()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();
    }
    public void AddPlayer(PlayerInput player)
    {
        players.Add(player);
        Transform playerPrefab = player.transform.parent;

        int layerToAdd = (int)Mathf.Log(playerLayers[players.Count - 1].value, 2);

        playerPrefab.GetComponentInChildren<CinemachineVirtualCamera>().gameObject.layer = layerToAdd;

        playerPrefab.GetComponentInChildren<Camera>().cullingMask |= 1 << layerToAdd;
        playerPrefab.GetComponentInChildren<Camera>().gameObject.layer = layerToAdd;
    }
}
