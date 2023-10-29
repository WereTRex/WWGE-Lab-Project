using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    private RectTransform _reticle;
    private List<CrosshairReticlePiece> _childPieces;


    [Header("Testing")]
    [Range(50, 250)]
    [SerializeField] private float _size = 100f;
    
    [Range(1, 5)]
    [SerializeField] private float _thickness = 2f;

    private void Start()
    {
        _reticle = GetComponent<RectTransform>();

        _childPieces = new List<CrosshairReticlePiece>();
        foreach (RectTransform child in _reticle)
        {
            if (child.TryGetComponent<CrosshairReticlePiece>(out CrosshairReticlePiece reticlePiece))
                _childPieces.Add(reticlePiece);
        }
    }


    private void Update()
    {
        SetReticleSize(_size);
        SetReticleThickness(_thickness);
    }

    public void SetReticleSize(float newSize)
    {
        _reticle.sizeDelta = new Vector2(newSize, newSize);
    }
    public void SetReticleThickness(float newThickness)
    {
        for (int i = 0; i < _childPieces.Count; i++)
        {
            _childPieces[i].SetPieceThickness(newThickness);
        }
    }
}
