using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    private Vector2 direction;
    private float slideSpeed;
    private float slideCooldown;

    private Animator Animator;
    private KeyBinding KeyBinding;
    private WeaponManagement _WeaponManagement;
    private Rigidbody2D _rigidbody2D;
    public Camera Camera;

    public State mouvementState;

    private void Start()
    {
        mouvementState = State.Walking;
        KeyBinding = GetComponent<KeyBinding>();
        Animator = GetComponent<Animator>();
        _WeaponManagement = GetComponent<WeaponManagement>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_rigidbody2D.velocity != Vector2.zero)
        {
            _rigidbody2D.velocity = Vector2.zero;
        }
        
        if (gameObject.GetComponent<PhotonView>().IsMine)
        {
            if (mouvementState == State.Walking)
            {                     
                InputKey();                    
                Move();                                         
            }
            else
                DodgeRoll();
        }
    }
    
    public enum State
    {
        Walking,
        Rolling
    }

    private void Move()
    {
        transform.Translate(Time.deltaTime * speed * direction);
        SetMovementAnim(direction, Camera);
        SetWeaponCoord(direction, Camera);
        if (slideCooldown > 0)
        {
            slideCooldown -= Time.deltaTime;
        }
    }

    private bool tryDodge()
    {
        return Physics.Raycast(transform.position, direction, 10, 8);
    }

    private void DodgeRoll()
    {
        transform.Translate(Time.deltaTime * slideSpeed * direction);
        slideSpeed -= slideSpeed * 2f * Time.deltaTime;
        if (slideSpeed <= 15 || tryDodge())
        {
            mouvementState = State.Walking;
            Animator.SetBool("Dodge", false);
            slideCooldown = 0.75f;
        }
    }

    //Si vous avez besoin d'ajouter un control, mettez le en majuscule.
    public void InputKey()
    {
        direction = Vector2.zero;

        if (Input.GetKey(KeyBinding.KeyCodes["UP"]))
            direction += Vector2.up;

        if (Input.GetKey(KeyBinding.KeyCodes["RIGHT"]))
            direction += Vector2.right;
            
        if (Input.GetKey(KeyBinding.KeyCodes["LEFT"]))
            direction += Vector2.left;

        if (Input.GetKey(KeyBinding.KeyCodes["DOWN"]))
            direction += Vector2.down;
        
        if (Input.GetKeyDown(KeyBinding.KeyCodes["FIRE"]))
            _WeaponManagement.fire();

        if (Input.GetKeyDown(KeyBinding.KeyCodes["RELOAD"]))
            _WeaponManagement.reloadOnPress();
            
        
        
        if (Input.GetKey(KeyBinding.KeyCodes["DODGE"]) && slideCooldown <= 0)
        {
            mouvementState = State.Rolling;
            slideSpeed = 25f;
            Animator.SetBool("Dodge", true);
        }
    }

    private void SetMovementAnim(Vector2 dir, Camera camera)
    {
        Vector2 BulletDir = (camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
            Input.mousePosition.y, -camera.transform.position.z)) - transform.root.position).normalized;
        
        Animator.SetFloat("xDir", dir.x);
        Animator.SetFloat("yDir", dir.y);
        Animator.SetFloat("xDirMouse", BulletDir.x);
    }

    private void SetWeaponCoord(Vector2 dir, Camera camera)
    {
        GameObject currWeapon = _WeaponManagement._inventory.currentWeapon;
        
        float currWeaponX = Mathf.Abs(_WeaponManagement._inventory.currWeaponX);
        float currWeaponY = _WeaponManagement._inventory.currWeaponY;
        float currWeaponScaleX = Mathf.Abs(_WeaponManagement._inventory.currWeaponScaleX);
        float currWeaponScaleY = Mathf.Abs(_WeaponManagement._inventory.currWeaponScaleX);
        
        Vector2 MouseDir = (camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
            Input.mousePosition.y, -camera.transform.position.z)) - transform.root.position).normalized;
        
        if (dir.x > 0)
        {
            currWeapon.transform.localPosition = new Vector3(currWeaponX, currWeaponY);
            currWeapon.transform.localScale = new Vector3(currWeaponScaleX, currWeaponScaleY);
        }
        else if (dir.x < 0)
        {
            currWeapon.transform.localPosition = new Vector3(-currWeaponX, currWeaponY);
            currWeapon.transform.localScale = new Vector3(-currWeaponScaleX, currWeaponScaleY);
        }
        else if (MouseDir.x > 0)
        {
            currWeapon.transform.localPosition = new Vector3(currWeaponX, currWeaponY);
            currWeapon.transform.localScale = new Vector3(currWeaponScaleX, currWeaponScaleY);
        }
        else
        {
            currWeapon.transform.localPosition = new Vector3(-currWeaponX, currWeaponY);
            currWeapon.transform.localScale = new Vector3(-currWeaponScaleX, currWeaponScaleY);
        }
        
    }
}
