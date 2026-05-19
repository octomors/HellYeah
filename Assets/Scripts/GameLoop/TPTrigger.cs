using UnityEngine;

public class TPTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        GameManager.Instance.CompleteFloor();
    }
}
