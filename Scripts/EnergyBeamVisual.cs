using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EnergyBeamVisual : MonoBehaviour
{
    public static EnergyBeamVisual Instance { get; private set; }

    [Header("References")]
    public Transform player;
    public PlayerChainPull pull;

    [Header("Base Wave")]
    public float waveAmplitude = 0.3f;
    public float waveFrequency = 6f;
    public float waveSpeed = 8f;

    [Header("Electric Effect")]
    public float electricAmplitude = 0.25f;
    public float electricFrequency = 22f;
    public float electricSpeed = 20f;
    public float electricWidthPulse = 0.06f;

    [Header("Resolution")]
    public int segmentCount = 16;

    LineRenderer line;
    Collider2D hookCollider;
    float time;
void Awake()
{
    Instance = this;


    line = GetComponent<LineRenderer>();
    line.useWorldSpace = true;
    line.enabled = false;
}

    void LateUpdate()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    void Update()
    {
        if (hookCollider == null || player == null)
        {
            line.enabled = false;
            line.positionCount = 0;
            return;
        }

        if (line.positionCount != segmentCount)
            line.positionCount = segmentCount;

        line.enabled = true;

        DrawBeam(player.position, hookCollider.bounds.center);
    }

    void DrawBeam(Vector2 start, Vector2 end)
    {
        Vector2 dir = end - start;
        Vector2 normal = new Vector2(-dir.y, dir.x).normalized;

        time += Time.deltaTime;

        bool electrified = pull != null && pull.IsPulling;

        line.startWidth = line.endWidth =
            electrified
                ? 0.15f + Mathf.Sin(Time.time * 40f) * electricWidthPulse
                : 0.15f;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);
            Vector2 pos = Vector2.Lerp(start, end, t);

            float baseWave =
                Mathf.Sin(t * waveFrequency * Mathf.PI * 2f + time * waveSpeed)
                * waveAmplitude * Mathf.Sin(t * Mathf.PI);

            float electricWave = electrified
                ? Mathf.Sin(t * electricFrequency * Mathf.PI * 2f + time * electricSpeed)
                  * electricAmplitude
                : 0f;

            line.SetPosition(i, pos + normal * (baseWave + electricWave));
        }
    }

    public void SetHook(Collider2D hook)
    {
        hookCollider = hook;
        time = 0f;

    }

    public void ClearHook(Collider2D hook)
    {
        if (hookCollider == hook)
        {
            hookCollider = null;
            time = 0f;
        }
    }
}
