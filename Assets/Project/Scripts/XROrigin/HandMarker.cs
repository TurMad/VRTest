using UnityEngine;

public class HandMarker : MonoBehaviour
{
    public SideEnum Side => side;
    [SerializeField] private SideEnum side;

    public enum SideEnum
    {
        Right,
        Left,
    }
}
