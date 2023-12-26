using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField] private Vector3 localSpawnPosition;
    public Vector3 SpawnPosition { get => (transform.position + transform.rotation * localSpawnPosition); }
    [field: SerializeField] public RepairableBarrier AssociatedBarrier { get; private set; }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(SpawnPosition, 0.5f);
    }
}
