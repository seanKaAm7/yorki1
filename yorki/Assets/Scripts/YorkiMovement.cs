using UnityEngine;

public class YorkiMovement : MonoBehaviour
{
    public float moveSpeed = 3f;

    // 방향 인덱스: 0=south, 1=south-east, 2=east, 3=north-east,
    //              4=north, 5=north-west, 6=west, 7=south-west
    private Animator _anim;

    void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        bool isMoving = (h != 0 || v != 0);

        if (isMoving)
        {
            Vector2 dir = new Vector2(h, v).normalized;
            transform.Translate(dir * moveSpeed * Time.deltaTime);
            _anim.SetInteger("dirIndex", GetDirIndex(h, v));
        }

        _anim.SetBool("isMoving", isMoving);
    }

    int GetDirIndex(float h, float v)
    {
        // 8방향 판별
        if (v < 0 && h == 0)  return 0; // south
        if (v < 0 && h > 0)   return 1; // south-east
        if (v == 0 && h > 0)  return 2; // east
        if (v > 0 && h > 0)   return 3; // north-east
        if (v > 0 && h == 0)  return 4; // north
        if (v > 0 && h < 0)   return 5; // north-west
        if (v == 0 && h < 0)  return 6; // west
        if (v < 0 && h < 0)   return 7; // south-west
        return 0;
    }
}
