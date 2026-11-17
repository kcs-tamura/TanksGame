using Mono.Cecil.Cil;
using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("移動,回転設定")]
    Vector2 _inputVer;           //入力方向
    float _curentNearDistance;   //現在の近い距離
    
    GameObject _nearPlayer;      //近くのPlayer  
    GameObject _previousPlayer;  //前回のPlayer

    public Vector2 InputVer => _inputVer;  //入力値を参照

    [Header("攻撃設定")]
    private Vector3 _moveTarget;
    private bool _hasMove = false;

    [Header("判定")]
    [SerializeField] private LayerMask _objectLayer;
    [SerializeField] private LayerMask _playerLayer;
    private float _direction = 1.5f; //Rayの距離
    Ray _ray; //Ray
    RaycastHit _hit; //RayHit情報

    //-----Component-----


    //-----Script-----
    MoveControlleer _move;            //移動Script
    private AtackController _atack;   //攻撃Script
    private StateManager _state;      //ステートScript
    private SensorController _sensor; //センサーScript

    void Start()
    {
        _move  = GetComponent<MoveControlleer>();
        _atack = GetComponent<AtackController>();
        _state = GetComponent<StateManager>();
        _sensor = GetComponentInChildren<SensorController>();
    }

    // Update is called once per frame
    void Update()
    {
        /*
         *-----BOTの行動パターン-----
         * 行動攻撃処理に移行できるのか
         * 　|-攻撃中return
         * 一番近くのPlayerを探査,登録
         *   |
         * ターゲット追跡
         *   |-ターゲットとの距離が指定以上中は追跡
         * チャージに移行
         *   |-距離が指定以上はチャージ
         * 移動停止,ターゲット方向に回転
         *   |-センサーに触れるまで回転
         * センサーがターゲットに触れたら攻撃
         *   |-
         *   
         */
        switch(_state._botState)
        {
            case BotState.None:
                //移動停止
                OnMove(Vector2.zero);
                if(_inputVer == Vector2.zero)
                {
                    _state.SetBotState(BotState.Search);
                    Debug.Log("None");

                }
                break;
            case BotState.Search:
                SearchPlayer();
                Debug.Log("Search");
                break;
            case BotState.Chase:
                Chase(_nearPlayer.transform.position);
                Debug.Log("Chase");
                break;
            case BotState.Charge:
                Charge(_nearPlayer.transform.position);
                Debug.Log("Charge");
                break;
            case BotState.Attack:
                break;
            case BotState.Cooldown:
                break;
            case BotState.Dead:
                break;
        }  
    }

    /// <summary>
    /// Rayを作りObjの判定
    /// </summary>
    /// <returns></returns>
    bool CheckObj()
    {
        var origin = transform.position + new Vector3(0, 0.5f, 0);
        var dir = transform.forward;
        _ray = new Ray(origin, dir);
        Debug.DrawRay(origin, dir * _direction, Color.red);

        return Physics.Raycast(_ray, out _hit, _direction, _objectLayer);
    }

    /// <summary>
    /// 最も近いプレイヤーを検索し、nearPlayer と curentNearDistance を更新する。
    /// </summary>
    /// <remarks>自身および previousPlayer を除外し、GameManager.Instance.playerList の各プレイヤーとの距離を Vector3.Distance
    /// で比較する。最短距離が見つかれば nearPlayer に割り当て、curentNearDistance を更新する。処理開始時に curentNearDistance は Mathf.Infinity
    /// で初期化される。</remarks>
    void SearchPlayer()
    {
        //初期化
        _curentNearDistance = Mathf.Infinity;
        _nearPlayer = null;
        var myPos = transform.position;

        //PlayerListの中で一番近いPlayerを検索,取得
        //foreach (var p in JoinDataHolder.instance.GetPlayerData.Keys)
        foreach (var p in Manager.Instance.TargetList)
        {
            //除外処理(自身,null,前回のターゲット,死亡者)
            if (p == gameObject || p == null || p == _previousPlayer) continue;
            var life = p.GetComponent<HitController>();
            if(life != null)
            {
                // 死亡しているなら除外
                if (!life.IsAlive) continue;
            }

            //2点間の距離計算
            var dist = p.transform.position - myPos;
            //大小判別二乗の距離が出る
            var sqrDist = dist.sqrMagnitude;

            //curentNearDistanceより小さい場合登録
            if (sqrDist < _curentNearDistance)
            {
                _curentNearDistance = sqrDist;
                _nearPlayer = p;
            }
        }
        if(_nearPlayer != null)
        {
            _state.SetBotState(BotState.Chase);
        }
        else
        {
            _state.SetBotState(BotState.None);
            return;
        }
    }

    /// <summary>
    /// 追跡処理
    /// </summary>
    void Chase(Vector3 targetPosition)
    {
        if (Distance(targetPosition) <= 6f)
        {
            OnMove(Vector2.zero);
            _state.SetBotState(BotState.Charge);
            return;
        }

        //相手までの角度計算
        float angle = Vector3.SignedAngle(transform.forward,
                                          Direction(targetPosition), 
                                          Vector3.up);

        if(Mathf.Abs(angle) > 5f){
            //回転
            OnMove(new Vector2(Mathf.Sign(angle), 0f));
        }
        else
        {
            //正面移動
            OnMove(new Vector2(0, 1));
        }
    }

    void Charge(Vector3 targetPosition)
    {
        if(_nearPlayer == null) {
            _atack.Attack(AttackState.Charge);
            _state.SetBotState(BotState.Search);
            return; 
        }

        _atack.Attack(AttackState.Charge);
        Chase(targetPosition);

        if (Distance(targetPosition) <= 3f)
        {
            OnMove(Vector2.zero);
            _state.SetBotState(BotState.Attack);
        }
    }

    void Attack(Vector3 targetPosition)
    {
        if(_nearPlayer == null)
        {
            _state.SetBotState(BotState.Search);
            return;
        }

        _atack.Attack(AttackState.Atatck);

        
    }

    /// <summary>
    /// 二点間の距離
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    float Distance(Vector3 targetPosition)
    {
        var dist = targetPosition - transform.position;
        dist.y = 0f;
        return dist.magnitude;
    }

    /// <summary>
    /// ターゲット方向(正規化)
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    Vector3 Direction(Vector3 targetPosition)
    {
        var dir = targetPosition - transform.position;
        dir.y = 0;
        dir.Normalize();
        return dir; 
    }

    /// <summary>
    /// 入力値などの変更
    /// </summary>
    /// <param name="context"></param>
    void OnMove(Vector2 context)
    {
        _move.SetMoveInput(context);
        _inputVer = context;
    }

    /// <summary>
    /// 移動,攻撃処理を停止
    /// </summary>
    /// <param name="x"></param>
    public void OnMoveStop(bool x)
    {
        if (_move == null || _atack == null) { return; }
        _move.enabled = x;
        _atack.enabled = x;
    }
}
