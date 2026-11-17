using UnityEngine;

public class MoveControlleer : MonoBehaviour
{
    [Header("移動,回転設定")]
    [SerializeField] private float _speed = 5f;//移動速度

    [SerializeField] private float _rotaSpeed = 10.0f;//回転速度

    Vector2 _inputVer;  //移動入力
    Rigidbody _rb;

    private StateManager _stateManager;

    private void Awake()
    {

        //取得
        _stateManager = GetComponent<StateManager>();
        _rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// 入力受け取り
    /// </summary>
    /// <param name="input"></param>
    public void SetMoveInput(Vector2 input)
    {
        _inputVer = input;
    }

    private void FixedUpdate()
    {
        if (_stateManager._state == State.Rigid) { return; }

        //攻撃以外の時の処理
        if (_stateManager._attackState != AttackState.Charge)
        {
            //移動処理
            Vector3 move = transform.forward * _inputVer.y * _speed * Time.fixedDeltaTime;
            _rb.MovePosition(_rb.position + move);
        }
        //回転処理
        var rota = _inputVer.x * _rotaSpeed * Time.fixedDeltaTime;
        var Rot = Quaternion.Euler(0,rota,0);
        _rb.MoveRotation(_rb.rotation * Rot);
        _stateManager.UpdateMoveState(_inputVer);
    }
}
