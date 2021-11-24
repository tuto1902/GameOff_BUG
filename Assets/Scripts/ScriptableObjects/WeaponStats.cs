using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Weapon Stats", fileName = "WeaponStats")]
public class WeaponStats : ScriptableObject
{
    public float damageAmount = 1f;
    public float knockbackForce = 10f;
}
