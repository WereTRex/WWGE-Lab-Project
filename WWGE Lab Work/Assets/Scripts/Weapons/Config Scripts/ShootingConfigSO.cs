using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Shooting Config", menuName = "Weapons/Guns/Shooting Config", order = 2)]
public class ShootingConfigSO : ScriptableObject
{
    public FiringType FiringType;

    [Space(5)]

    public bool IsHitscan = true;
    public Bullet BulletPrefab;
    public float BulletLaunchForce = 1000f;

    [Space(5)]

    public LayerMask HitMask;
    public float FireDelay = 0.25f;


    [Header("Spread")]
    [Min(1)] public int BulletsPerShot = 1;
    [Range(0f, 180f)]
    public float MaxBulletAngle = 0f;
    public bool UseWeightedSpread = false;


    [Header("Recoil")]
    public float RecoilRecoverySpeed = 1.5f;
    public float MaxSpreadTime = 1f;
    [Range(0, 1)]public float MinSpreadPercent = 0.1f;

    [Space(5)]

    [Tooltip("An AnimationCurve for the size of the dynamic crosshair depending on the current firing time")]
        public AnimationCurve CrosshairCurve;

    [Space(5)]

    [Header("Simple Spread")]
    // With how it is currently set, using a Vector3 would change the value based on the rotation of the player.
    public float MaxSpreadAngle = 0.1f;


    public Vector3 GetSpread(float shootTime = 0)
    {
        float spreadRadius = Mathf.Tan((MaxSpreadAngle / 2f) * Mathf.Deg2Rad);
        Vector3 spreadDirection = Vector3.Lerp(
            a: Vector3.zero,
            b: Random.insideUnitCircle * spreadRadius,
            t: Mathf.Clamp(shootTime / MaxSpreadTime, MinSpreadPercent, 1f));

        return spreadDirection;
    }

    // Get the spread direction from a Texture2D.
    /*private Vector3 GetTextureDirection(float shootTime)
    {
        Vector2 halfSize = new Vector2(SpreadTexture.width / 2f, SpreadTexture.height / 2f);
        
        // The size of the square that we are going to sample.
        int halfSquareExtents = Mathf.CeilToInt(
            Mathf.Lerp(
                0.01f,
                halfSize.x,
                Mathf.Clamp(shootTime / MaxSpreadTime, MinSpreadPercent, 1f))
            );

        // Get the bottom left corner of this square.
        int minX = Mathf.FloorToInt(halfSize.x) - halfSquareExtents;
        int minY = Mathf.FloorToInt(halfSize.y) - halfSquareExtents;


        // Sample the rectangle of pixels from the bottom left corner to the top right.
        // Note: Generates some Garbage, though this should be fine with only a few agents.
        Color[] sampleColours = SpreadTexture.GetPixels(
            minX,
            minY,
            halfSquareExtents * 2,
            halfSquareExtents * 2);

        // Convert the sampled colours to greyscale.
        float[] coloursAsGrey = System.Array.ConvertAll(sampleColours, (colour) => colour.grayscale);
        
        // Get a Weighted Random value between 0 & the total Grey Value.
        float totalGreyValue = coloursAsGrey.Sum();
        float grey = Random.Range(0, totalGreyValue);

        // Find the index of the colour that we wish to use.
        int i = 0;
        for (; i < coloursAsGrey.Length; i++)
        {
            grey -= coloursAsGrey[i];
            if (grey <= 0)
                break;
        }


        // Map this index back to the 2D Texture Coord on the Sampled Texture (Gives columns).
        int x = minX + i % (halfSquareExtents * 2);
        int y = minY + i % (halfSquareExtents * 2);

        // Get the direction to the target position on the texture.
        Vector2 targetPosition = new Vector2(x, y);
        Vector2 direction = (targetPosition - halfSize) / halfSize.x;


        return direction;
    }*/
}
