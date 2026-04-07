using UnityEngine;
using UnityEngine.UI;
using System;





[Serializable]
public class TowerPosition
{
    [Header("📍 References")]
    public GameObject Spot;
    public Button Button;
    public Text CostText;

    [Header("⚙️ Settings")]
    public int TowerLevel = 1;
    public int TowerCost = 100;
    public bool IsEnabled = true;

    [Header("🔧 Runtime")]
    [NonSerialized] public bool IsOccupied = false;
    [NonSerialized] public int TowerEntityId = -1;


    public Vector3 Position => Spot != null ? Spot.transform.position : Vector3.zero;


    public int GetUpgradeCost() => Mathf.FloorToInt(TowerCost * 0.5f);


    public bool CanPlace(int playerGold)
    {
        if (!IsEnabled) return false;
        
        if (IsOccupied)
        {

            return TowerLevel < 3 && playerGold >= GetUpgradeCost();
        }
        else
        {

            return playerGold >= TowerCost;
        }
    }
}