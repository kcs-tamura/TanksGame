using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class AtackController : MonoBehaviour
{
    [Header("攻撃設定")]
    [SerializeField] private GameObject _bullet;//弾Prefab
    private float _cooldown = 1.0f;          //攻撃クールダウン
    //-----チャージ-------
    private const float _chargeMax = 1.0f;   //Maxチャージ量
    private float _curentCharge = 0f;        //現在のチャージ量
  
    private GameObject _curentBullet;

    //-----硬直---------
    private float _recoveryTime = 0.5f;//硬直時間

    private AtackPower _curentPower = default;   //現在の攻撃


    Rigidbody _rb;
    StateManager _stateManager;

    private void Awake()
    {
        //初期化

        //取得
        _rb = GetComponent<Rigidbody>();
        _stateManager = GetComponent<StateManager>();
    }

    /// <summary>
    /// チャージゲージの同期
    /// </summary>
    /// <param name="value"></param>
    public void SetCharge(float value, Vector3 attackPos = default(Vector3))
    {
        _curentCharge = Mathf.Clamp01(value);
    }

    private void Update()
    {
        //maxなら強攻撃
        if (_curentCharge >= _chargeMax)
        {
            _stateManager.SetAttackPower(AtackPower.Strong);
        }
    }

    /// <summary>
    /// チャージ,攻撃処理
    /// </summary>
    /// <param name="x"></param>
    public void Attack(AttackState state)
    {
        if (!enabled) {return; }
        //チャージ開始(ステートをチャージ中に)
        if (state == AttackState.Charge)
        {
            if (_stateManager._attackState == AttackState.Cooldown || _stateManager._attackState == AttackState.Charge) { return; }
            _stateManager.SetAttackState(state);
            _stateManager.SetBotState(BotState.Charge); 
        }
        //攻撃開始
        if (state == AttackState.Atatck && _curentBullet == null)
        {
            if (_stateManager._attackState == AttackState.Cooldown) { return; }

            if (_stateManager._attackState == AttackState.Charge)
            {
                _stateManager.SetAttackState(state);
                _stateManager.SetBotState(BotState.Attack);
                //attackPowerステートがStrongならstrongPower,それ以外ならweakPower
                _curentPower = _stateManager._attackPower;

                var y = transform.eulerAngles.y;
                _curentBullet = Instantiate(_bullet,new Vector3(transform.position.x,0.5f,transform.position.z + 1f),Quaternion.Euler(0,y,0));
                StartCoroutine(RigidCount());
                var p = _curentBullet.GetComponent<BulletController>();
                if(p != null)
                {
                    p.SetAttackController(this,y);
                    p.Shot(_curentBullet.transform.position, _curentPower);
                    p.SetOwner(gameObject);
                }
            }
        }
    }

    public void BulletDestory()
    {
        _stateManager.SetAttackState(AttackState.Atatck);
        _stateManager.SetBotState(BotState.Attack);
        _curentBullet = null;
        StartCoroutine(CooldownCount());
    }

    /// <summary>
    /// クールダウン処理
    /// </summary>
    /// <returns></returns>
    IEnumerator CooldownCount()
    {
        _stateManager.SetAttackState(AttackState.Cooldown);
        _stateManager.SetBotState(BotState.Cooldown);
        yield return new WaitForSeconds(_cooldown);
        _stateManager.SetAttackState(AttackState.None);
        _stateManager.SetBotState(BotState.None);
    }

    IEnumerator RigidCount()
    {
        _stateManager.SetState(State.Rigid);
        yield return new WaitForSeconds(_recoveryTime);
        _stateManager.SetState(State.None);
    }
}


