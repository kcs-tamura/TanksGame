using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public enum Reflection
{
    None,
    Zero,
    One,
}

public class BulletController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _exprosionPrefab;

    private float speed = 5f;

    private Reflection _reflection = Reflection.None;
    
    public GameObject _Owner { get; private set; }

    [SerializeField] private LayerMask _hitLayer;
    [SerializeField] private LayerMask _playerHitLayer;

    Rigidbody _rb;
    AtackController _attack;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

   private void FixedUpdate()
    {
        if(_reflection == Reflection.None) { return; }

        var move = transform.forward * speed * Time.fixedDeltaTime;
        _rb.MovePosition(transform.position + move);
    }

    public void Shot(Vector3 startPos, AtackPower state)
    {
        transform.position = startPos;
        if(state == AtackPower.Strong)
        {
            _reflection = Reflection.One;
        }
        else
        {
            _reflection = Reflection.Zero;
        }
    }

    void Bouns(/*Vector3 normal*/)
    {
        var rot = transform.rotation;
        var curentY = rot.eulerAngles.y;
        if (curentY == 0)
        {
            curentY = 180;
        }

        transform.rotation = Quaternion.Euler(0, -curentY, 0f);

        /*        Vector3 reflectedDirection = Vector3.Reflect(transform.forward, normal).normalized;

                reflectedDirection.y = 0f;

                transform.rotation = Quaternion.LookRotation(
                    reflectedDirection,
                    Vector3.up);*/
    }

    /// <summary>
    /// AttackController取得,方向取得
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="a"></param>
    public void SetAttackController(AtackController controller ,float a)
    {
        _attack = controller;
    }

    /// <summary>
    /// 生成者を認識
    /// </summary>
    /// <param name="owner"></param>
    public void SetOwner(GameObject owner)
    {
        _Owner = owner;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & _hitLayer) != 0)
        {
            if(_reflection == Reflection.One)
            {
                Bouns(/*collision.contacts[0].normal*/);
                _reflection = Reflection.Zero;
                return;
            }
            _attack.BulletDestory();
            var obj = Instantiate(_exprosionPrefab, transform.position, Quaternion.identity);
            obj.Play();
            Destroy(this.gameObject);
        }

        if (((1 << collision.gameObject.layer) & _playerHitLayer) != 0)
        {
            _attack.BulletDestory();
            var obj = Instantiate(_exprosionPrefab, transform.position, Quaternion.identity);
            obj.Play();
            Destroy(this.gameObject);
        }
    }
}
