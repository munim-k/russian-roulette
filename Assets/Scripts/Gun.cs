using System.Collections;
using UnityEngine;


public class Gun : MonoBehaviour
{
    public void SpinGun(int playerNumber, int spins, float time, bool add = false, int totalPlayers = 6) {
        float targetDegree = 360 - (360f / totalPlayers * playerNumber);
        SpinGun(targetDegree, spins, time, add);
    }

    void SpinGun(float targetDegree, int totalSpins, float spinTime, bool add = false)
    {
        StartCoroutine(SpinCoroutine(targetDegree, totalSpins, spinTime, add));
    }

    private IEnumerator SpinCoroutine(float targetDegree, int totalSpins, float spinTime, bool add = false)
    {
        float elapsedTime = 0f;
        float startRotation = transform.eulerAngles.z;
        if (add) {
            startRotation = 0;
        }
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

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, startRotation + totalRotation);
    }
}