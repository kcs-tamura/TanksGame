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
  
    private GameObject _curentBullet;

    //-----硬直---------
    private float _recoveryTime = 0.5f;//硬直時間

    private AtackMode _curentPower = default;   //現在の攻撃


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
    /// チャージ,攻撃処理
    /// </summary>
    /// <param name="x"></param>
    public void Attack(AttackState state, AtackMode mode)
    {
        if (!enabled) {return; }
    　　//Aim
        if (state == AttackState.Aim)
        {
            if (_stateManager._attackState == AttackState.Cooldown || _stateManager._attackState == AttackState.Aim) { return; }
            _stateManager.SetAttackState(state); 
        }
        //攻撃開始
        if (state == AttackState.Atatck && _curentBullet == null)
        {
            if (_stateManager._attackState == AttackState.Cooldown) { return; }

            if (_stateManager._attackState == AttackState.Aim)
            {
                _stateManager.SetAttackMode(mode);

                _stateManager.SetAttackState(state);
                //attackPowerステートがStrongならstrongPower,それ以外ならweakPower
                _curentPower = _stateManager._attackPower;

                var y = transform.eulerAngles.y;
                var spawnPosition = transform.position + transform.forward;
                spawnPosition.y = 0.5f;
                _curentBullet = Instantiate(_bullet, spawnPosition, Quaternion.Euler(0, y, 0));
                StartCoroutine(RigidCount());
                var p = _curentBullet.GetComponent<BulletController>();
                if(p != null)
                {
                    p.SetAttackController(this,y);
                    p.Shot(_curentBullet.transform.position, _curentPower);
                    //p.SetOwner(gameObject);
                }
            }
        }
    }

    public void BulletDestory()
    {
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


