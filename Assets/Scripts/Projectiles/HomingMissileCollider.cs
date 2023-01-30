using UnityEngine;

public class HomingMissileCollider : MonoBehaviour
{
    public void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == Constants.PlayerTag)
        {
            transform.parent.GetComponent<HomingMissileController>().ExplodeWithDamage();
        }
    }
}
