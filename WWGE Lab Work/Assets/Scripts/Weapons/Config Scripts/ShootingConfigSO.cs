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


    [Header("Recoil & Spread")]
    public float RecoilRecoverySpeed = 1f;
    public float MaxSpreadTime = 1f;

    [Space(5)]

    public BulletSpreadType SpreadType = BulletSpreadType.Simple;
    [Header("Simple Spread")]
    // With how it is currently set, using a Vector3 would change the value based on the rotation of the player.
    public float MaxSpread = 0.1f;

    [Header("Texture-Based Spread")]
    [Range(0.001f, 5f)]
    public float SpreadMultiplier = 0.1f;
    public Texture2D SpreadTexture;


    public Vector3 GetSpread(float shootTime = 0)
    {
        Vector3 spread = Vector3.zero;

        if (SpreadType == BulletSpreadType.Simple)
        {
            spread = Vector3.Lerp(
                Vector3.zero,
                new Vector3(
                    x: Random.Range(-MaxSpread, MaxSpread),
                    y: Random.Range(-MaxSpread, MaxSpread),
                    z: Random.Range(-MaxSpread, MaxSpread)),
                Mathf.Clamp01(shootTime / MaxSpreadTime));
        }
        else if (SpreadType == BulletSpreadType.TextureBased)
        {
            spread = GetTextureDirection(shootTime) * SpreadMultiplier;
        }

        return spread;
    }

    // Get the spread direction from a Texture2D.
    private Vector3 GetTextureDirection(float shootTime)
    {
        Vector2 halfSize = new Vector2(SpreadTexture.width / 2f, SpreadTexture.height / 2f);
        
        // The size of the square that we are going to sample.
        int halfSquareExtents = Mathf.CeilToInt(
            Mathf.Lerp(
                0.01f,
                halfSize.x,
                Mathf.Clamp01(shootTime / MaxSpreadTime))
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
    }
}