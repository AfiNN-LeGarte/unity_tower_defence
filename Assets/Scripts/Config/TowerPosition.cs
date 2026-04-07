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

    [Header("⚙️ Config")]
    public int Index;
    public bool IsEnabled = true;

    public Vector3 Position => Spot != null ? Spot.transform.position : Vector3.zero;
}
