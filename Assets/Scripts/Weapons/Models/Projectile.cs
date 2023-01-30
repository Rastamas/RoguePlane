using UnityEngine;

public class Projectile
{
    public Projectile(GameObject prefab)
    {
        this.prefab = prefab;

        var capsuleCollider = prefab.GetComponent<CapsuleCollider>();

        if (capsuleCollider != null)
        {
            colliderOffset = capsuleCollider.center * prefab.transform.localScale.y;
            return;
        }

        var boxCollider = prefab.GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            colliderOffset = boxCollider.center * prefab.transform.localScale.y;
            return;
        }
    }

    public GameObject owner { get; set; }
    public GameObject prefab { get; set; }
    public bool isEnemy { get; set; }
    public int damage { get; set; }

    // The collider is offset from the actual model to look like
    // it's fired from below the player, but to still have it hit
    // the collider of the enemies    
    public Vector3 colliderOffset { get; }
}
