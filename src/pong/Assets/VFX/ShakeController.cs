using System.Collections;
using UnityEngine;

public class ShakeController : MonoBehaviour
{
    [SerializeField]
    private float shakeAmount;

    private bool isShaking = false;

    private Vector3 originalPosition;

    private void Start()
    {
        this.originalPosition = transform.position;
    }

    private void Update()
    {
        if (this.isShaking)
        {
            Vector3 newPosition = this.originalPosition + Random.insideUnitSphere * (Time.deltaTime * shakeAmount);
            newPosition.y = this.transform.position.y;
            newPosition.z = this.transform.position.z;

            this.transform.position = newPosition;
        }
    }

    public void Shake()
    {
        this.StartCoroutine(this.ShakeNow());
    }

    private IEnumerator ShakeNow()
    {
        if (!this.isShaking)
        {
            this.isShaking = true;
        }

        yield return new WaitForSeconds(0.25f);

        this.isShaking = false;
        this.transform.position = this.originalPosition;
    }
}
