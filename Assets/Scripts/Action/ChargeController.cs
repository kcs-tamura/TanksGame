using UnityEngine;
using UnityEngine.UI;

public class ChargeController : MonoBehaviour
{
    [SerializeField] private float _maxChargeTime = 1.5f;
    [SerializeField] private float _maxPos        = 500f;
    [SerializeField] private Image _pointImage;
    //position
    RectTransform _pos;
    Vector2 _initPos = new Vector2(0, 250);


    private AtackController _ac;
    private StateManager _stateManager;

    private void Awake()
    {
        _ac = GetComponent<AtackController>();
        _stateManager = GetComponent<StateManager>();

        _pos = _pointImage.rectTransform;
        _pos.anchoredPosition = _initPos;
    }

    private void Update()
    {
        if(_stateManager._attackState == AttackState.Atatck) { return; }

        if (_stateManager._attackState == AttackState.Charge)
        {
            _pointImage.enabled = true;
            float speed = (_maxPos - _initPos.y) / _maxChargeTime;
            float height = _pos.anchoredPosition.y;
            height += speed * Time.deltaTime;

            height = Mathf.Clamp(height, _initPos.y, _maxPos);

            _pos.anchoredPosition = new Vector2(_initPos.x,height);
            float chargeRate = (height - _initPos.y) / (_maxPos - _initPos.y);
            _ac.SetCharge(chargeRate, _pointImage.transform.position);
        }
        else
        {
            _pos.anchoredPosition = _initPos;
            _ac.SetCharge(0);
            _pointImage.enabled = false;
        } 
    }
}
