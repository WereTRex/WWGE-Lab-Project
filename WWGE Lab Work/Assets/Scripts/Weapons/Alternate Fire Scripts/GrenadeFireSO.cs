using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Grenade Alt Fire", menuName = "Weapons/Guns/Alternate Fire/Grenade")]
public class GrenadeFireSO : AlternateFireSO
{
    [Space(5)]
    public GameObject GrenadePrefab;
    public float LaunchForce;
    
    public override void AlternateAttack(GameObject triggeringGameObject, Transform raycastOrigin, Transform bulletOrigin)
    {
        GameObject grenadeInstance = Instantiate(GrenadePrefab, bulletOrigin.position, bulletOrigin.rotation);
        grenadeInstance.GetComponent<Rigidbody>().AddForce(raycastOrigin.forward * LaunchForce, ForceMode.Impulse);
    }
}
