using UnityEngine;

public class KitchenZoneVolume : MonoBehaviour
{
    [SerializeField] private BoxCollider zoneCollider;

    private void Reset()
    {
        zoneCollider = GetComponent<BoxCollider>();
    }

    public bool Contains(Vector3 worldPosition)
    {
        if (zoneCollider == null)
            return false;

        return zoneCollider.bounds.Contains(worldPosition);
    }
}