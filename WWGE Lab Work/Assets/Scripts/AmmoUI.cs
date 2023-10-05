using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private TMPro.TMP_Text _ammoText;


    private void Update()
    {
        if (!_weaponManager.GetIsReloading())
            _ammoText.text = _weaponManager.GetCurrentAmmo() + "/" + _weaponManager.GetMaxClipAmmo();
        else
            _ammoText.text = "...";
    }
}
