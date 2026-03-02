using Unity.Mathematics;
using UnityEngine;

public class delete : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(math.sin(Time.time), math.cos(Time.time));
    }
}
