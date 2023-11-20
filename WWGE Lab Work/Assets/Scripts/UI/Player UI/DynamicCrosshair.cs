using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicCrosshair : MonoBehaviour
{
    private RectTransform _reticle;
    [SerializeField] private WeaponManager _connectedWeaponManager;

    [SerializeField] private List<CrosshairReticlePiece> _reticlePieces;

    [Space(5)]

    [SerializeField] private Color _defaultCrosshairColour = Color.yellow;

    [SerializeField] private float _defaultReticleThickness = 1f;
    [SerializeField] private Color _defaultBorderColour = Color.black;


    private void Start()
    {
        _reticle = GetComponent<RectTransform>();

        InitializeReticleValues();
    }
    private void Update() => SetCrosshairValues();

    private void InitializeReticleValues()
    {
        PlayerManager.Instance.CrosshairColour = _defaultCrosshairColour;

        PlayerManager.Instance.CrosshairBorderThickness = _defaultReticleThickness;
        PlayerManager.Instance.CrosshairBorderColour = _defaultBorderColour;
    }


    // We should find a more efficient way to receive this, rather than doing this all every frame.
    private void SetCrosshairValues()
    {
        SetReticleSize(_connectedWeaponManager.GetCrosshairSize());

        foreach (CrosshairReticlePiece reticlePiece in _reticlePieces)
        {
            reticlePiece.SetPieceColour(PlayerManager.Instance.CrosshairColour);
            
            reticlePiece.SetBorderThickness(PlayerManager.Instance.CrosshairBorderThickness);
            reticlePiece.SetBorderColour(PlayerManager.Instance.CrosshairBorderColour);
        }
    }

    private void SetReticleSize(float newSize)
    {
        _reticle.sizeDelta = new Vector2(newSize, newSize);
    }
}
