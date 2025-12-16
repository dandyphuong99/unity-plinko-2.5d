using System.Collections;
using TikTokLiveSharp.Events.Objects;
using TikTokLiveUnity;
using TikTokLiveUnity.Utils;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public enum Team { Nam, Nu }
    [Header("Random Bounce Settings")]
    [SerializeField] private float randomXoayStart = 0.05f;     // Mô-men xoắn ngẫu nhiên
    [SerializeField] private float randomNgangStart = 0.2f;    // Lực ngang ngẫu nhiên
    [SerializeField] private float lucBacPeg = 0.1f;    // Lực bật peg
    [SerializeField] private float lucXoayKhiCham = 0.3f;    // Lực xoay khi chạm

    [Header("Sound Settings")]
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;

    private Quaternion initialRotation; // lưu rotation gốc
    private float destroyY = -35f;
    private float avatarYOffset = 1.2f;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private bool canFall = false;
    private SpriteRenderer avatarSprite;
    private SpriteRenderer outlineSprite; // viền avatar
    public Team team = Team.Nam;

    public bool isScored { get; private set; }


    private void Awake()
    {
        initialRotation = transform.rotation; // Lưu rotation gốc của prefab
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        DisableBallFall();
        CreateAvatarSprite();
    }

    private void Update()
    {
        CheckDestroyCondition();
    }

    public void GetScope(int scopeValue)
    {
        if (isScored) return; // tránh cộng điểm nhiều lần
        if (team == Team.Nam)
            ScopeManager.Instance.AddScoreNam(scopeValue);
        else
            ScopeManager.Instance.AddScoreNu(scopeValue);
        isScored = true;
    }

    /// <summary>
    /// Tạo sprite renderer cho avatar
    /// </summary>
    private void CreateAvatarSprite()
    {

        // Tạo object cho avatar
        GameObject avatarObj = new GameObject("Avatar");
        avatarObj.transform.SetParent(transform);
        avatarObj.transform.localPosition = Vector3.zero;
        avatarObj.transform.localScale = Vector3.one;
        Rigidbody2D avatarRb = avatarObj.AddComponent<Rigidbody2D>();
        avatarRb.bodyType = RigidbodyType2D.Kinematic;
        avatarSprite = avatarObj.AddComponent<SpriteRenderer>();
        avatarSprite.sortingOrder = 10;
    }

    /// <summary>
    /// Đặt ball ở trạng thái kinematic (không chịu tác động vật lý)
    /// </summary>
    private void DisableBallFall()
    {
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    /// <summary>
    /// Khởi tạo avatar từ picture
    /// </summary>
    /// <param name="avatarPicture">Picture object chứa thông tin avatar</param>
    public void InitAvatar(Picture avatarPicture, Team gender)
    {
        CreateAvatarSprite();
        team = gender;
        string bestUrl = Utils.GetBestImageUrl(avatarPicture);
        if (string.IsNullOrEmpty(bestUrl))
        {
            Debug.LogWarning("Invalid avatar URL");
            return;
        }

        Dispatcher.RunOnMainThread(() => LoadAvatarSprite(bestUrl));
    }

    /// <summary>
    /// Tải và thiết lập sprite cho avatar
    /// </summary>
    /// <param name="imageUrl">URL của ảnh avatar</param>
    private void LoadAvatarSprite(string imageUrl)
    {
        if (!TikTokLiveManager.Exists)
        {
            Debug.LogError("TikTokLiveManager not available");
            return;
        }

        TikTokLiveManager.Instance.RequestSprite(imageUrl, OnSpriteLoaded);
    }

    /// <summary>
    /// Callback khi sprite được tải xong
    /// </summary>
    /// <param name="sprite">Sprite đã tải</param>
    private void OnSpriteLoaded(Sprite sprite)
    {
        if (avatarSprite == null || sprite == null)
        {
            Debug.LogError("Failed to load sprite: avatarSprite or sprite is null");
            return;
        }

        ApplyAvatarSprite(sprite);
    }

    /// <summary>
    /// Áp dụng sprite cho avatar
    /// </summary>
    /// <param name="sprite">Sprite cần áp dụng</param>
    private void ApplyAvatarSprite(Sprite sprite)
    {
        Texture2D circleTexture = Utils.CreateCircleTexture(sprite.texture);
        Sprite circleSprite = Utils.CreateSpriteFromTexture(circleTexture);

        avatarSprite.sprite = circleSprite;
        avatarSprite.transform.localPosition = new Vector3(0, avatarYOffset, 0);
        avatarSprite.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
    }


    /// <summary>
    /// Kích hoạt trạng thái rơi cho ball
    /// </summary>
    public void EnableFall()
    {
        StartCoroutine(EnableFallCoroutine());
    }

    /// <summary>
    /// Coroutine để kích hoạt rơi
    /// </summary>
    private IEnumerator EnableFallCoroutine()
    {
        yield return null; // Chờ 1 frame
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;

            // Thêm một chút lực ngang và torque
            float horizontalForce = Random.Range(-randomNgangStart, randomNgangStart);
            rb.AddForce(new Vector2(horizontalForce, 0), ForceMode2D.Impulse);

            float torque = Random.Range(-randomXoayStart, randomXoayStart);
            rb.AddTorque(torque, ForceMode2D.Impulse);
        }
        canFall = true;
    }

    /// <summary>
    /// Kiểm tra điều kiện hủy ball
    /// </summary>
    private void CheckDestroyCondition()
    {
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Kiểm tra nếu va chạm với Peg
        if (collision.gameObject.CompareTag("Peg"))
        {
            // Chỉnh volume theo lực va chạm (tùy chọn)
            float volume = Mathf.Clamp01(collision.relativeVelocity.magnitude / 10f);
            audioSource.volume = volume;
            // Chơi âm thanh
            audioSource.Play();

            // Lấy điểm va chạm
            ContactPoint2D contact = collision.contacts[0];

            // Vector pháp tuyến (hướng bật ra)
            Vector2 normal = contact.normal;

            // Thêm lực bật ra một chút theo hướng pháp tuyến
            rb.AddForce(normal * lucBacPeg, ForceMode2D.Impulse);

            // Độ lớn lực va chạm (vận tốc tương đối)
            float impactForce = collision.relativeVelocity.magnitude;
            // Thêm lực bật ra nhẹ theo hướng pháp tuyến để bóng không bị dính peg
            rb.AddForce(contact.normal * Mathf.Clamp(impactForce * lucBacPeg, 0.5f, 2f), ForceMode2D.Impulse);

            // Vector từ tâm bóng đến điểm va chạm
            Vector2 contactVector = contact.point - (Vector2)transform.position;
            // Tính torque dựa trên cross product (mô phỏng moment xoắn)
            float torque = Vector3.Cross(contactVector, collision.relativeVelocity).z;
            // Thêm torque xoay bóng (tăng hệ số để thấy rõ)
            rb.AddTorque(torque * lucXoayKhiCham, ForceMode2D.Impulse);
        }

        // Nếu va chạm với goal
        GoalZone goal = collision.gameObject.GetComponent<GoalZone>();
        if (goal != null)
        {
            Debug.Log("Scored in goal: " + goal.name);
            // if (team == Team.Nam)
            //     GameManager.Instance.AddScoreNam(goal.scoreValue);
            // else
            //     GameManager.Instance.AddScoreNu(goal.scoreValue);

            // rb.linearVelocity = Vector2.zero;     // Dừng bóng
            // rb.angularVelocity = 0f;        // Dừng xoay
            // transform.position = collision.contacts[0].point; // optional: đặt bóng lên trên Goal
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            // Xóa ball
            StartCoroutine(DeleteBall());
        }
    }

    private IEnumerator DeleteBall()
    {
        // Nuốt bóng (giảm scale dần trước khi destroy)
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        float sinkTime = 0.2f;
        while (elapsed < sinkTime)
        {
            if (transform == null) break; // phòng trường hợp bị Destroy sớm
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / sinkTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (transform != null)
            Destroy(transform.gameObject);
    }


}