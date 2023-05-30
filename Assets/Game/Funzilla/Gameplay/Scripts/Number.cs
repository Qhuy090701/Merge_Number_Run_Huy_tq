using UnityEngine;

public class Number : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
            Debug.Log("Va chạm");
        }
    }
}
