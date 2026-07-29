using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    [Header("発射設定")]
    public float speedKmh = 70f;      // 初速 km/h
    public float upSpeed = 3f;        // 上向きの初速（m/s）

    [Header("初期位置設定")]
    // Aボタンを押す前にボールを置いておく位置（ワールド座標）
    // Inspectorで調整可能。キャッチャーから見て奥（+Z側）に置く
    public Vector3 startPosition = new Vector3(0f, 1.5f, 10f);
    public bool useStartPosition = true;  // trueなら上のstartPositionに強制配置

    private Rigidbody rb;
    private bool launched = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Rigidbodyが付いていません！ BallにRigidbodyを追加してください。");
            return;
        }

        // 発射前はその場に静止（重力でも落ちない）
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 初期位置に配置（見えない対策：確実に決めた場所に置く）
        if (useStartPosition)
        {
            transform.position = startPosition;
        }

        Debug.Log("BallLauncher started! ボール位置: " + transform.position +
                  " / Aボタンで発射します");
    }

    void Update()
    {
        // Aボタンを押したら発射（まだ発射していない場合のみ）
        if (!launched && OVRInput.GetDown(OVRInput.Button.One))
        {
            Launch();
        }
    }

    void Launch()
    {
        launched = true;
        rb.isKinematic = false;
        float speedMs = speedKmh / 3.6f;
        rb.linearVelocity = new Vector3(0f, upSpeed, -speedMs);  // -Z方向へ、少し上向き
        Debug.Log("===== 発射！ 速度=" + speedKmh + "km/h (" + speedMs.ToString("F2") + "m/s) =====");
    }
}