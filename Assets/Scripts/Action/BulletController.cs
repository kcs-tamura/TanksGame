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
    private float _angle = 0;

    private Reflection _reflection = Reflection.None;
    
    public GameObject _Owner { get; private set; }

    [SerializeField] private LayerMask _hitLayer;

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

    /// <summary>
    /// AttackController取得,方向取得
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="a"></param>
    public void SetAttackController(AtackController controller ,float a)
    {
        _attack = controller;
        _angle = a;
    }

    /// <summary>
    /// 生成者を認識
    /// </summary>
    /// <param name="owner"></param>
    public void SetOwner(GameObject owner)
    {
        _Owner = owner;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _hitLayer) != 0)
        {
            _attack.BulletDestory();
            var obj = Instantiate(_exprosionPrefab, transform.position, Quaternion.identity);
            obj.Play();
            Destroy(this.gameObject);
        }
    }
}
