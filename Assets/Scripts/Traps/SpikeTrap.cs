using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Cài đặt tia quét (Raycast)")]
    public float rayDistance = 5f; // Khoảng cách tia đỏ
    public Vector2 attackDirection = Vector2.up; // Hướng quét tia đỏ và hướng gai lao tới
    public LayerMask playerLayer; // Layer của Player để tia quét phát hiện

    [Header("Cài đặt chuyển động của Spike")]
    public float attackDistance = 2f; // Độ dài tối đa mà gai có thể lao tới
    public float riseSpeed = 15f; // Tốc độ đâm tới
    public float returnSpeed = 5f; // Tốc độ thụt về
    public float delayBeforeReturn = 1f; // Thời gian chờ trước khi thụt về

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool isTriggered = false;
    private bool returning = false;
    private float returnTimer;

    void Update()
    {
        Vector2 normalizedDirection = attackDirection.normalized;

        // Vẽ tia đỏ trong tab Scene để dễ tinh chỉnh dọc theo hướng tấn công
        Debug.DrawRay(transform.position, normalizedDirection * rayDistance, Color.red);

        // NẾU GAI CHƯA BỊ KÍCH HOẠT
        if (!isTriggered)
        {
            // Bắn tia (raycast) theo hướng attackDirection để tìm Player
            RaycastHit2D hit = Physics2D.Raycast(transform.position, normalizedDirection, rayDistance, playerLayer);
            
            if (hit.collider != null)
            {
                // Kiểm tra xem tia có chạm trúng Player không
                if (hit.collider.CompareTag("Player") || hit.collider.GetComponent<PlayerController>() != null)
                {
                    isTriggered = true; // Kích hoạt gai lao lên
                    // Tính original và target position ngay lúc kích hoạt để đảm bảo luôn mới nhất
                    originalPosition = transform.position;
                    targetPosition = originalPosition + (Vector3)normalizedDirection * attackDistance;
                }
            }
        }
        else // KHI GAI ĐÃ BỊ KÍCH HOẠT
        {
            if (!returning) // Đang đâm tới
            {
                // Di chuyển Spike lên vị trí target
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, riseSpeed * Time.deltaTime);

                // Nếu đã lao tới đích
                if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
                {
                    returnTimer += Time.deltaTime;
                    // Đợi delay rồi bắt đầu thụt xuống
                    if (returnTimer >= delayBeforeReturn)
                    {
                        returning = true;
                        returnTimer = 0f;
                    }
                }
            }
            else // Đang thụt xuống
            {
                // Di chuyển Spike về vị trí gốc
                transform.position = Vector3.MoveTowards(transform.position, originalPosition, returnSpeed * Time.deltaTime);

                // Nếu đã về tới vị trí cũ
                if (Vector3.Distance(transform.position, originalPosition) < 0.01f)
                {
                    isTriggered = false; // Sẵn sàng kích hoạt lại
                    returning = false;
                }
            }
        }
    }

    // Xử lý khi Spike chạm vào Player (nếu Spike có Collider dạng Solid)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Respawn();
        }
    }

    // Xử lý khi Spike chạm vào Player (nếu Spike có Collider dạng Trigger)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Respawn();
        }
    }
}
