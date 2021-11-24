using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private WeaponStats stats;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Vector2 knockbackDirection = other.transform.position - transform.position;
            knockbackDirection.Normalize();
            other.GetComponent<EnemyController>().TakeDamage(stats.damageAmount, knockbackDirection * stats.knockbackForce);
        }
    }
}
