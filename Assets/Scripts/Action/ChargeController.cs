using UnityEngine;
using UnityEngine.UI;

public class ChargeController : MonoBehaviour
{
    [SerializeField] private float _maxChargeTime = 1.5f;
    [SerializeField] private Image _gaugeImage;
    private float meterSpeed = 1.0f;
    private Coroutine meter;

    private AtackController _ac;
    private StateManager _stateManager;

    private void Awake()
    {
        _ac = GetComponent<AtackController>();
        _stateManager = GetComponent<StateManager>();
        //初期化
        _gaugeImage.fillAmount = 0;
    }

    private void Update()
    {
        float speed = 1f / _maxChargeTime;

        if (_stateManager._attackState == AttackState.Aim)
        {
            _gaugeImage.fillAmount += speed * Time.deltaTime;
        }
        else
        {
            _gaugeImage.fillAmount = 0;
        }

        //_ac.SetCharge(_gaugeImage.fillAmount);
    }
}
