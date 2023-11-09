using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCrosshair : MonoBehaviour
{
    private RectTransform _reticle;
    [SerializeField] private WeaponManager _connectedWeaponManager;


    private void Start() => _reticle = GetComponent<RectTransform>();
    private void Update() => SetReticleSize(_connectedWeaponManager.GetCrosshairSize()); // We should find a more efficient way to receive this, rather than going through two/three scripts every frame.


    private void SetReticleSize(float newSize)
    {
        _reticle.sizeDelta = new Vector2(newSize, newSize);
    }
}
