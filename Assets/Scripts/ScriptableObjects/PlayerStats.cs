using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player Stats", fileName = "PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("Movement")]
	public float moveSpeed;
	public float acceleration;
	public float deceleration;
	public float velPower;
	[Space(10)]
	public float frictionAmount;
	[Space(10)]
	public bool canMove = true;

	[Header("Health")]
	public float maxHealth;
}
