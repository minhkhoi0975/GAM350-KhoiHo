/**
 * HitboxManager.cs
 * Description: The Hitbox class stores information about hitboxes.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    public GameObject gameObject;
    public float health;
    
    public Character(GameObject aGameObject, float aHealth)
    {
        gameObject = aGameObject;
        health = aHealth;
    }

    public static implicit operator GameObject(Character characterData)
    {
        return characterData.gameObject;
    }
}

public class Projectile
{
    public GameObject gameObject;
    public float damage;

    public Projectile(GameObject aGameObject, float aDamage)
    {
        gameObject = aGameObject;
        damage = aDamage;
    }

    public static implicit operator GameObject(Projectile projectile)
    {
        return projectile.gameObject;
    }
}

[System.Serializable]
public class HitboxData
{
    // Prefab for projectile hitboxes.
    [SerializeField] GameObject projectileHitboxPrefab;
    public GameObject ProjectileHitboxPrefab
    {
        get
        {
            return projectileHitboxPrefab;
        }
    }

    // Prefab for character hitboxes (including the shooters and the NPCs).
    [SerializeField] GameObject characterHitboxPrefab;
    public GameObject CharacterHitboxPrefab
    {
        get
        {
            return characterHitboxPrefab;
        }
    }

    // Hitboxes of projectiles.
    Dictionary<int, Projectile> projectileHitboxes = new Dictionary<int, Projectile>();
    public Dictionary<int, Projectile> ProjectileHitboxes
    {
        get
        {
            return projectileHitboxes;
        }
    }

    // Hitboxes of shooters.
    Dictionary<int, Character> shooterHitboxes = new Dictionary<int, Character>();
    public Dictionary<int, Character> ShooterHitboxes
    {
        get
        {
            return shooterHitboxes;
        }
    }

    // Hitboxes of NPCs.
    Dictionary<int, Character> npcHitboxes = new Dictionary<int, Character>();
    public Dictionary<int, Character> NPCHitboxes
    {
        get
        {
            return npcHitboxes;
        }
    }
}
