using UnityEngine;
using System;

[Serializable]
public class TowerPosition
{
    public GameObject Spot;
    public Renderer SphereRenderer;
    public UnityEngine.UI.Button Button;
    public UnityEngine.UI.Text CostText;

    public int Index;
    public bool IsEnabled = true;

    public Vector3 Position => Spot != null ? Spot.transform.position : Vector3.zero;
}
