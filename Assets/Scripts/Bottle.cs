using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Bottle : NetworkBehaviour
{

    [SerializeField] GameObject shootButton; // To enable after spin 
    [SerializeField] GameObject gun; // To display gun

    void Start() {
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void SpinBottle(int playerNumber, int spins, int time, int totalPlayers = 6) {
        float targetDegree = 360 - (360f / totalPlayers * playerNumber);
        SpinBottle(targetDegree, spins, time);
    }

    public void SpinBottle(float targetDegree, int totalSpins, float spinTime)
    {
        StartCoroutine(SpinCoroutine(targetDegree, totalSpins, spinTime));
    }

    private IEnumerator SpinCoroutine(float targetDegree, int totalSpins, float spinTime)
    {
        float elapsedTime = 0f;
        float startRotation = transform.eulerAngles.z;
        float totalRotation = 360f * totalSpins + targetDegree;

        while (elapsedTime < spinTime)
        {
            float t = elapsedTime / spinTime;
            t = Mathf.SmoothStep(0, 1, t); // Smooth the rotation

            float currentRotation = Mathf.Lerp(startRotation, startRotation + totalRotation, t);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, currentRotation);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the bottle lands exactly on the target degree
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, startRotation + totalRotation);
        shootButton.SetActive(true);
        // gameObject.SetActive(false);
        gun.SetActive(true);
        gun.transform.rotation = gameObject.transform.rotation;
    }
}