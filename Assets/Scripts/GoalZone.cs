using System.Collections;
using UnityEngine;

public class GoalZone : MonoBehaviour
{
    public int scoreValue = 10;       // Điểm của ô
    public float sinkDistance = 0.2f; // Mức chìm khi nuốt bóng
    public float sinkTime = 0.2f;     // Thời gian chìm

    private AudioSource audioSource;
    private Vector3 originalPos;

    void Awake()
    {
        originalPos = transform.position;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball != null && !ball.isScored)
        {
            // Chơi âm thanh nuốt
            audioSource.Play();

            // Cộng điểm cho đội
            ball.GetScope(scoreValue);

            // Bắt Coroutine chìm
            StartCoroutine(Sink());
        }
    }

    //Hiệu ứng chìm
    private IEnumerator Sink()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = originalPos - new Vector3(0, sinkDistance, 0);

        // Chìm goal trong sinkTime
        while (elapsed < sinkTime)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / sinkTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;

        


        // Reset goal về vị trí cũ (tùy muốn)
        transform.position = originalPos;
    }
}
