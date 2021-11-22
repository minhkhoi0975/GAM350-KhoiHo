/**
 * HitboxManager.cs
 * Description: The Hitbox class stores information about hitboxes.
 * Programmer: Khoi Ho
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHitbox
{
    public GameObject hitboxGameObject;
    public float health;

    public CharacterHitbox(GameObject aGameObject, float aHealth)
    {
        hitboxGameObject = aGameObject;
        health = aHealth;
    }

    public static implicit operator GameObject(CharacterHitbox characterData)
    {
        return characterData.hitboxGameObject;
    }
}

public class ProjectileHitbox
{
    public GameObject hitboxGameObject;
    public float damage;

    // The network ID of the character that fires the projectile.
    public int instigatorNetworkId;

    public ProjectileHitbox(GameObject aGameObject, float aDamage, int aInstigatorNetworkId)
    {
        hitboxGameObject = aGameObject;
        damage = aDamage;
        instigatorNetworkId = aInstigatorNetworkId;
    }

    public static implicit operator GameObject(ProjectileHitbox projectile)
    {
        return projectile.hitboxGameObject;
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
    Dictionary<int, ProjectileHitbox> projectileHitboxes = new Dictionary<int, ProjectileHitbox>();
    public Dictionary<int, ProjectileHitbox> ProjectileHitboxes
    {
        get
        {
            return projectileHitboxes;
        }
    }

    // Hitboxes of shooters.
    Dictionary<int, CharacterHitbox> shooterHitboxes = new Dictionary<int, CharacterHitbox>();
    public Dictionary<int, CharacterHitbox> ShooterHitboxes
    {
        get
        {
            return shooterHitboxes;
        }
    }

    // Hitboxes of NPCs.
    Dictionary<int, CharacterHitbox> npcHitboxes = new Dictionary<int, CharacterHitbox>();
    public Dictionary<int, CharacterHitbox> NPCHitboxes
    {
        get
        {
            return npcHitboxes;
        }
    }
}