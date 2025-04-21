using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rong : MonoBehaviour
{
    public GameObject _lua;
    public Transform _danlua;
    private float _time;
    private Animator _animator;
    // Start is called before the first frame update
    void Start()
    {
       _animator  = GetComponent<Animator>();
       _time = 0 ;

    }

    // Update is called once per frame
    void Update()
    {
        _time += Time.deltaTime;
        if(_time > 3f)
        {
            _animator.SetTrigger("atk");
            _time = 0;
        }
    }

    private void lua ()
    {
        Instantiate( _lua, _danlua.position, Quaternion.identity, transform);
    }
}
