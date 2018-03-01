using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class Movement : MonoBehaviour {
    public string keyup;
    public string keydown;
    public string keyleft;
    public string keyright;
    public string keyattack;
    private PlayerInput PI;
    

    //Movement Variables
    public float hspeed = 30;
    public float frictionWhenToFast = 11;
    public float jumppower = 600;

    //Needed Physicsobjects
    public Rigidbody RB;
    public Transform Trans;

    //Double / Downjump
    public int doubleJump = 2;
    public int NumberOfDoubleJumps = 2;
    public int downJump = 1;

    public float SpecialMove = 0.8f;
    public int MaxSizeSpecialMove = 6;
    public int AttackForceMultiplyer = 600;
    private bool SpecialMoveAvalable = true;
    private bool SpecialMoveActive = false;
    private bool slowedDownY = false;
    private bool slowedDownX = false;

    //Slowdown Variables
    public float maxspeed = 20;
    public float trigger_maxdistance = 200;
    private Vector2 distancetraveled = new Vector2(0,0);
    private Vector2 previousPosition;

    private Vector3 attfc;
    //10 ist the perfect number. It is used to slow down the User the faster he gets
    //basicly setting the maximum Speed
    private int MovementKeeperValue = 10;

    private Vector3 OriginalScale;
    void Start()
    {
        attfc = new Vector3();
        OriginalScale = Trans.localScale;
        PI = new PlayerInput(keyup, keydown, keyleft, keyright, keyattack);
        previousPosition = Trans.position;
    }

    void Update () {

        PI.getPlayerInput();
        bool onground = Physics.Raycast(Trans.position, Vector3.down, 0.8f);

        StandardMovement(PI, onground);
        AttackMovement(PI, onground);
        SlowDownIfToFast();
        
    }
    public void StandardMovement(PlayerInput PI, bool onground)
    {
        Vector3 fc = new Vector3();
        
        

        if (PI.left && !SpecialMoveActive) fc.x -= hspeed + RB.velocity.x * 2;
        if (PI.right && !SpecialMoveActive) fc.x += hspeed - RB.velocity.x * 2;

        if (onground)
        {
            doubleJump = NumberOfDoubleJumps;
            downJump = 1;
        }

        if (PI.up && onground && !PI.attack && SpecialMoveAvalable)
            fc.y += jumppower - RB.velocity.y * jumppower / MovementKeeperValue;
        else if (PI.up && doubleJump > 0 && !PI.attack && SpecialMoveAvalable)
        {
            fc.y += jumppower - RB.velocity.y * jumppower / MovementKeeperValue;
            doubleJump -= 1;
            downJump = 1;
        }

        if (PI.up_release && RB.velocity.y > 0)
        {
            RB.AddForce(0, -RB.velocity.y * 25, 0);
        }

        if (PI.down && !onground && downJump > 0 && !PI.attack && !SpecialMoveActive)
        {
            //the 1.4f just minders the effect
            fc.y -= (jumppower + RB.velocity.y * jumppower / MovementKeeperValue);
            downJump--;
        }
        //Apply MovementForce
        RB.AddForce(fc);
    }
    
    public void AttackMovement(PlayerInput PI, bool onground)
    {
        //Attackmove
        if (PI.attackclick && SpecialMoveAvalable)
        {
            SpecialMoveAvalable = false;
            SpecialMoveActive = true;
        }

        if (PI.attack && SpecialMoveActive)
        {

            float addingScale = SpecialMove * Convert.ToSingle(Math.Pow(OriginalScale.x / Trans.localScale.x, 1.25));
            if (Trans.localScale.x > MaxSizeSpecialMove) addingScale = 0;


            Trans.localScale += new Vector3(addingScale, addingScale, addingScale);

        }


        if (PI.attackrelease && SpecialMoveActive)
        {
            float attackforcenumber = hspeed / 10;

            attfc = new Vector3();

            if (PI.up_hold) attfc.y += attackforcenumber;
            if (PI.down_hold) attfc.y -= attackforcenumber;
            if (PI.right) attfc.x += attackforcenumber;
            if (PI.left) attfc.x -= attackforcenumber;
            if (!PI.up_hold && !PI.down_hold && !PI.right && !PI.left)
            {
                if (RB.velocity.x > 0) attfc.x += attackforcenumber;
                if (RB.velocity.x < 0) attfc.x -= attackforcenumber;
                if (RB.velocity.y > 0) attfc.y += attackforcenumber;
                if (RB.velocity.y < 0) attfc.y -= attackforcenumber;
            }

            float EffectDämpfen = 0.12f;
            if (attfc.y > 0 && RB.velocity.y < 0) attfc.y = attfc.y + -1 * RB.velocity.y * EffectDämpfen;
            if (attfc.y < 0 && RB.velocity.y > 0) attfc.y = attfc.y + -1 * RB.velocity.y * EffectDämpfen;
            if (attfc.x > 0 && RB.velocity.x < 0) attfc.x = attfc.x + -1 * RB.velocity.x * EffectDämpfen;
            if (attfc.x < 0 && RB.velocity.x > 0) attfc.x = attfc.x + -1 * RB.velocity.x * EffectDämpfen;

            attfc *= (Trans.localScale.x * Trans.localScale.x * Trans.localScale.x) / 64;
            attfc *= AttackForceMultiplyer;


            RB.AddForce(attfc);

            SpecialMoveActive = false;
        }

        if (!SpecialMoveActive && !PI.attack && Trans.localScale.x > OriginalScale.x)
        {
            float subtractScale = SpecialMove / Trans.localScale.x;

            //Even out the Movement in the Air
            //fc.x -= hspeed + RB.velocity.x*2
            //This is Accelarating
            //attfc = attfc + RB.velocity;



            Trans.localScale -= new Vector3(subtractScale, subtractScale, subtractScale);



        }
        if (Trans.localScale.x < OriginalScale.x) Trans.localScale = OriginalScale;
        if (Trans.localScale.x == OriginalScale.x && onground)
        {
            slowedDownX = false;
            slowedDownY = false;
            SpecialMoveAvalable = true;
        }


    }

    public void SlowDownIfToFast()
    {
        //Stop the Player from getting to fast
        Vector2 SlowdownBySpeed = new Vector2(-RB.velocity.x, -RB.velocity.y);

        if(!SpecialMoveAvalable && !slowedDownX)
        {
            if (RB.velocity.x > maxspeed)
            {
                RB.AddForce(frictionWhenToFast * SlowdownBySpeed.x, 0, 0);
                slowedDownX = true;
                Debug.Log("slowingdown");
            }
            if (RB.velocity.x < -maxspeed)
            {
                RB.AddForce(frictionWhenToFast * SlowdownBySpeed.x, 0, 0);
                slowedDownX = true;
                Debug.Log("slowingdown");
            }

            if (RB.velocity.y > maxspeed)
            {
                RB.AddForce(0, frictionWhenToFast * SlowdownBySpeed.y, 0);
                slowedDownY = true;
                Debug.Log("slowingdown");
            }


            if (RB.velocity.y < -maxspeed)
            {
                RB.AddForce(0, frictionWhenToFast * SlowdownBySpeed.y, 0);
                slowedDownY = true;
                Debug.Log("slowingdown");
            }
            
            
            //Add to traveleddistance
            /*if (previousPosition.x > Trans.position.x)
            {
                distancetraveled.x += previousPosition.x  Trans.position.x
            }
            else
            {

            }

            if (previousPosition.y > Trans.position.y)
            {

            }
            else
            {

            }
            */
        }


       
    }

}

public class PlayerInput
{
    public bool left;
    public bool right;

    public bool up_release;
    public bool up;
    public bool up_hold;

    public bool down;
    public bool down_hold;

    public bool attackclick;
    public bool attack;
    public bool attackrelease;

    private string keyup;
    private string keydown;
    private string keyleft;
    private string keyright;
    private string keyattack;

    public PlayerInput(string keyup, string keydown, string keyleft, string keyright, string keyattack)
    {
        this.keyup = keyup;
        this.keydown = keydown;
        this.keyleft = keyleft;
        this.keyright = keyright;
        this.keyattack = keyattack;
    }

    public void getPlayerInput()
    {
        this.left = Input.GetKey(keyleft);
        this.right = Input.GetKey(keyright);

        this.up_release = Input.GetKeyUp(keyup);
        this.up = Input.GetKeyDown(keyup);
        this.up_hold = Input.GetKey(keyup);

        this.down = Input.GetKeyDown(keydown);
        this.down_hold = Input.GetKey(keydown);

        this.attackclick = Input.GetKeyDown(keyattack);
        this.attack = Input.GetKey(keyattack);
        this.attackrelease = Input.GetKeyUp(keyattack);
    }
}

