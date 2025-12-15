using UnityEngine;

[DisallowMultipleComponent]
public class Facing2D : MonoBehaviour
{
    [Header("If true: +ScaleX faces Right, If false: +ScaleX faces Left")]
    [SerializeField] private bool positiveScaleFacesRight = false;

    private float cachedAbsScaleX = 1.0f;

    private void Awake()
    {
        float absX = Mathf.Abs(transform.localScale.x);

        if (absX > 0.0001f)
        {
            cachedAbsScaleX = absX;
        }
    }

    public int GetFacingDir()
    {
        float sx = transform.localScale.x;
        int sign = (sx >= 0.0f) ? 1 : -1;

        if (positiveScaleFacesRight == true)
        {
            return sign;
        }

        return -sign;
    }

    public void SetFacingDir(int dir)
    {
        int d = (dir >= 0) ? 1 : -1;

        float absX = cachedAbsScaleX;

        if (absX <= 0.0001f)
        {
            absX = Mathf.Abs(transform.localScale.x);

            if (absX <= 0.0001f)
            {
                absX = 1.0f;
            }

            cachedAbsScaleX = absX;
        }

        float x = d * absX;

        if (positiveScaleFacesRight == false)
        {
            x = -x;
        }

        Vector3 s = transform.localScale;
        s.x = x;
        transform.localScale = s;
    }
}