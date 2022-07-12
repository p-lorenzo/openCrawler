using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject inventory;
    [SerializeField] private Rig rig;
    private bool isInventoryOpen = false;
    [SerializeField] private bool isPistolEquipped = false;

    private void Awake()
    {
        inventory.SetActive(false);
    }

    public void OnMenu()
    {
        isInventoryOpen = !isInventoryOpen;
        inventory.SetActive(isInventoryOpen);
    }

    private void Update()
    {
        if (isPistolEquipped)
        {
            Debug.Log("rig weight 1");
            rig.weight = 1;
        } else
        {
            Debug.Log("rig weight 0");
            rig.weight = 0;
        }
    }
}
