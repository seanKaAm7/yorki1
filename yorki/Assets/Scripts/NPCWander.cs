using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class NPCWander : MonoBehaviour
{
    public float wanderRadius = 3f;
    public float moveSpeed = 1.2f;
    public float idleTimeMin = 1f;
    public float idleTimeMax = 3.5f;
    public Color npcColor = Color.gray;

    private Vector2 _origin;
    private SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();

        if (_sr.sprite == null)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        _sr.color = npcColor;
        _sr.sortingOrder = 1;
    }

    void Start()
    {
        _origin = transform.position;
        StartCoroutine(WanderLoop());
    }

    IEnumerator WanderLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(idleTimeMin, idleTimeMax);
            yield return new WaitForSeconds(waitTime);

            Vector2 dest = _origin + Random.insideUnitCircle * wanderRadius;

            while (Vector2.Distance(transform.position, dest) > 0.05f)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    dest,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }
        }
    }
}
