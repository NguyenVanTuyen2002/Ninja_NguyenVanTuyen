using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float speed = 5;
    [SerializeField] private float jumpForce = 350;
    [SerializeField] bool isGrounded = true;
    [SerializeField] private Kunai kunaiPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private GameObject attackArea;

    bool isJumping = false;
    bool isAttack = false;
    private bool isDeath = false;
    float horizontal;
    private int coin = 0;
    private Vector3 savePoint;

    private void Awake()
    {
        coin = PlayerPrefs.GetInt("coin", 0);
    }

    void Update()
    {
       
        if (IsDead)
        {
            return;
        }


        //Debug.Log(CheckGround());
        isGrounded = CheckGround();

        //horizontal = Input.GetAxisRaw("Horizontal");

        if (isAttack)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if(isGrounded)
        {
            if (isJumping)
            {
                return;
            }

            //jump
            if (Input.GetKeyDown(KeyCode.W) && isGrounded)
            {
                Jump();
            }

            //change anim run
            if (Mathf.Abs(horizontal) > 0.1f)
            {
                ChangeAnim("run");
            }

            //attack
            if (Input.GetKeyDown(KeyCode.C) && isGrounded)
            {
                Attack();
            }

            //throw
            if (Input.GetKeyDown(KeyCode.V) && isGrounded)
            {
                Throw();
            }
        }

        //check falling
        if (!isGrounded && rb.velocity.y < 0)
        {
            //Debug.Log("Falling");
            ChangeAnim("fall");
            isJumping = false;
        }

        //Moving
        if (Mathf.Abs(horizontal) > 0.1f)
        {
            //ChangeAnim("run");
            rb.velocity = new Vector2 (horizontal * speed, rb.velocity.y);
            transform.rotation = Quaternion.Euler(new Vector3(0, horizontal > 0 ? 0 : 180, 0));
        }
        else if(isGrounded)        
        {
            //Debug.Log("Idle");
            ChangeAnim ("idle");
            rb.velocity = Vector2.zero;
        }
    }

    public override void OnInit()
    {
        base.OnInit();
        isDeath = false; 
        isAttack = false;
        transform.position = savePoint;
        ChangeAnim("idle");
        DeActiceAttack();
        SavePoint();
        UIManager.instance.SetCoin(coin);
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        OnInit();
    }

    protected override void OnDeath()
    {
        base.OnDeath();
    }
    private bool CheckGround()
    {
        Debug.DrawLine(transform.position, transform.position + Vector3.down * 1.1f, Color.red);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, groundLayer);
        
        /*if (hit.collider != null )
        {
            return true;
        }
        else
        { 
            return false;
        }*/

        return hit.collider != null;
    }

    public void Attack()
    {
        Debug.Log("Attack");
        ChangeAnim("attack");
        isAttack = true;
        Invoke(nameof(ResetAttack), 0.5f);
        ActiceAttack();
        Invoke(nameof(DeActiceAttack), 0.5f);
    }

    public void Throw()
    {
        ChangeAnim("throw");
        isAttack = true;
        Invoke(nameof(ResetAttack), 0.5f);

        Instantiate(kunaiPrefab, throwPoint.position, throwPoint.rotation);
    }

    private void ResetAttack()
    {
        Debug.Log("reset");
        isAttack = false;
        ChangeAnim("idle");
    }

    public void Jump()
    {
        //Debug.Log("Jump");
        isJumping = true;
        ChangeAnim("jump");
        rb.AddForce(jumpForce * Vector2.up);
    }

    internal void SavePoint()
    {
        savePoint = transform.position;
    }
    private void ActiceAttack()
    {
        attackArea.SetActive(true);
    }

    private void DeActiceAttack()
    {
        attackArea.SetActive(false);
    }

    public void SetMove(float horizontal)
    {
        this.horizontal = horizontal;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            coin++;
            PlayerPrefs.SetInt("coin", coin);
            UIManager.instance.SetCoin(coin);
            Destroy(collision.gameObject);
        }

        if (collision.tag == "Deathzone")
        {
            ChangeAnim("die");
            Invoke(nameof(OnInit), 1f);
        }
    }
}