using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    [SerializeField] private TMP_Text _hitRigidbodiesText;
    private int _hitObjectCount = 0;


    private void OnEnable() => WeaponManager.OnPlayerHitPhysicsObject += IncreaseHitObjectCount;
    private void OnDisable() => WeaponManager.OnPlayerHitPhysicsObject -= IncreaseHitObjectCount;


    void IncreaseHitObjectCount()
    {
        _hitObjectCount += 1;
        _hitRigidbodiesText.text = _hitObjectCount.ToString();
    }
}
