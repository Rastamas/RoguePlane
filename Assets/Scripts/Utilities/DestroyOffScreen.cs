using UnityEngine;

public class DestroyOffScreen : MonoBehaviour
{
    private EnemySpawner _playField;

    public void Start()
    {
        _playField = EnemySpawner.instance;
    }

    public void FixedUpdate()
    {
        if (MovedOffScreen)
        {
            var dropController = GetComponent<DropController>();
            if (dropController != null)
            {
                dropController.Disable();
            }

            var onDestoryComponent = GetComponent<IEnemyDestroyed>();

            onDestoryComponent?.OnEnemyDestroyed();

            Destroy(gameObject);
        }
    }

    public bool MovedOffScreen => transform.position.z < (_playField.transform.position.z - _playField.spawnZOffset * 1.5f) ||
        transform.position.z > (_playField.transform.position.z + _playField.spawnZOffset * 2f);
}
