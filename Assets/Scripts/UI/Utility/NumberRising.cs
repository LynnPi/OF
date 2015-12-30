using UnityEngine;
using System.Collections;
using System.Globalization;

public class NumberRising : MonoBehaviour {
	public delegate  void NumberRisingObserver(ulong number);	
	System.Action _onRisingCompleted;

	float _lifeTime;
	float _deltaTimeForEffect = 0.01f;
	bool _canRise = false;
	bool _isRising = false;

	float _number;
	public float Target {
		set{ 
			_number = value;
		} 
	}
	public void SyncNumber(ulong number){
		_number = number;
		if (_observer != null){
			_observer(number);
		}
	}

	public bool IsRising{
		get {
			return _isRising;
		}
	}
	
	NumberRisingObserver _observer;
	public NumberRisingObserver Observer { set { _observer = value; } }
	
	public void Play(float lifeTime, ulong to, System.Action OnRisingCompleted = null){
		_onRisingCompleted = OnRisingCompleted;

		//DebugUtils.Assert(lifeTime > 0f, "LifeTime must be larger than 0 !");
		//DebugUtils.Assert(_observer != null, "NumberRising : observer == null");
		
		_lifeTime = lifeTime + _deltaTimeForEffect;
		
		if(_isRising) StopCoroutine("Lerp");
		
		_canRise = true;	
		_isRising = true;
		StartCoroutine("Lerp", to);
	}

	public void Stop(){
		_canRise = false;
		_isRising = false;
	}
	
	IEnumerator Lerp(ulong to){
		float origin = _number;
		//DebugUtils.Assert(to >= (ulong)_number, string.Format("to {0} < current {1}", to, (ulong)_number));
		ulong length = to - (ulong)_number;
		float timeElapsed = 0f;

		while(timeElapsed < _lifeTime){
			if(!_canRise){
				SyncNumber(to);
				if(_onRisingCompleted != null) _onRisingCompleted();
				yield break;
			}

			timeElapsed += Time.deltaTime;
			float current = origin + length * (timeElapsed / _lifeTime);
			ulong display = (ulong)current;
			if (display >= to) display = to;
			SyncNumber(display);
			yield return null;
		}

		SyncNumber(to);
		_isRising = false;
		if(_onRisingCompleted != null) _onRisingCompleted();
	}
}