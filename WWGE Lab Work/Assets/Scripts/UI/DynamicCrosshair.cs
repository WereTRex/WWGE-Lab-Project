using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCrosshair : MonoBehaviour
{
    private RectTransform _reticle;
    [SerializeField] private WeaponManager _connectedWeaponManager;


    private void Start() => _reticle = GetComponent<RectTransform>();
    private void Update() => SetReticleSize(_connectedWeaponManager.GetCrosshairSize()); // We should find a more efficient way to receive this, rather than through two/three references.


    private void SetReticleSize(float newSize)
    {
        Debug.Log(newSize);
        _reticle.sizeDelta = new Vector2(newSize, newSize);
    }
}
