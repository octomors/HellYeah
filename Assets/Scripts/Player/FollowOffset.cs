using UnityEngine;
public class FollowOffset : MonoBehaviour
{
    public Transform target;

    private Vector3 offset;

    void Start()
    {
        offset = transform.position - target.position;
    }

    void Update()
    {
        transform.position = target.position + offset;
    }
}