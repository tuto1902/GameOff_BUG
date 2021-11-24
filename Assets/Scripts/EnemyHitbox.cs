using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [SerializeField] private WeaponStats stats;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Vector2 knockbackDirection = other.transform.position - transform.position;
            knockbackDirection.Normalize();
            other.GetComponent<PlayerController>().TakeDamage(stats.damageAmount, knockbackDirection * stats.knockbackForce);
        }
    }
}
