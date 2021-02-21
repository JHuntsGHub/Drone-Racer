using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 *  This class is for each player's quadcopters. It inherits most of it's functionality from Quadcopter as the ai will also avail of it's contents.
 */
public class Player : Quadcopter {

    /*
     *  The public (ie Dragged in from Unity) variables the Player class needs are as follows:
     *      > playerNum is determines which controller the player uses.
     *      > healthText, scoreText, livesText  -->  These are for the UI.
     *      > camera1st, camera3rd  -->  These are the 1st and 3rd person camera views.
     *      > wifi_Strong, wifi_Medium, wifi_Weak, wifi_Gone  -->  The images used for signal strength.
     *      > centreOfMap is a game object that allows us to determine the distance from the centre of the map.
     */
    public int playerNum;
    public Text healthText, scoreText, boostText, livesText;
    public Camera camera1st, camera3rd;
    public Sprite wifi_Strong, wifi_Medium, wifi_Weak, wifi_Gone;
    public Image signalStrengthImage;
    public GameObject centreOfMap;

    /*
     * The private variable maxDistanceFromCenter allows us to calculate the signal strength from the centreOfMap.
     */
    private const float maxDistanceFromCenter = 400f;

    /*
     *  The Start() method inherits most of it's contents from Quadcopter.
     *  The onle new functionallity is starting the players' view in 3rd person mode.
     */
    protected override void Start()
    {
        base.Start();
        camera1st.enabled = false;
        camera3rd.enabled = true;
        rotarSounds.volume = 0;
    }

    /*
     *  The Update method simply calls the methods the player needs to function.
     */
    protected override void Update()
    {
        base.Update();
        CameraSwap();
        CalculateSignalStrength();
    }

    protected override void DoMovement()
    {
        //calculate rotation with yaw, pitch, roll.
        Vector3 rotation = new Vector3(Input.GetAxis("Pitch" + playerNum), -Input.GetAxis("Yaw" + playerNum), -Input.GetAxis("Roll" + playerNum));
        transform.Rotate(rotation * rotationSpeed * Time.deltaTime);

        /*
         *  This quadcopter only has one way of moving, thrusting with the rotars.
         *  This can give you upward OR downward velocity with varying speeds thanks to a controllers' axis.
         */
        rigidB.AddForce(transform.up * -Input.GetAxis("Thrust" + playerNum) * speedForce * Time.deltaTime);

        if (!GameController.cheatsOn)
        {
            /*
             *  The following resets the angular velocity of the quadcopter. It reduces/raises it over time until it reaches 0.
             *  This happens passively after a crash.
             */
            if (rigidB.angularVelocity.x > 0)
                rigidB.angularVelocity = new Vector3(rigidB.angularVelocity.x - angularVelocityMinuser * Time.deltaTime, rigidB.angularVelocity.y, rigidB.angularVelocity.z);
            else if (rigidB.angularVelocity.x < 0)
                rigidB.angularVelocity = new Vector3(rigidB.angularVelocity.x + angularVelocityMinuser * Time.deltaTime, rigidB.angularVelocity.y, rigidB.angularVelocity.z);
            if (rigidB.angularVelocity.y > 0)
                rigidB.angularVelocity = new Vector3(rigidB.angularVelocity.x, rigidB.angularVelocity.y - angularVelocityMinuser * Time.deltaTime, rigidB.angularVelocity.z);
            else if (rigidB.angularVelocity.y < 0)
                rigidB.angularVelocity = new Vector3(rigidB.angularVelocity.x, rigidB.angularVelocity.y + angularVelocityMinuser * Time.deltaTime, rigidB.angularVelocity.z);
            if (rigidB.angularVelocity.z > 0)
                rigidB.angularVelocity = new Vector3(rigidB.angularVelocity.x, rigidB.angularVelocity.y, rigidB.angularVelocity.z - angularVelocityMinuser * Time.deltaTime);
            else if (rigidB.angularVelocity.z < 0)
                rigidB.angularVelocity = new Vector3(rigidB.angularVelocity.x, rigidB.angularVelocity.y, rigidB.angularVelocity.z + angularVelocityMinuser * Time.deltaTime);

            /*
             *  The following cancels the angular velocity when the corrosponding control is used.
             */
            if (Input.GetAxis("Pitch" + playerNum) != 0)
                rigidB.angularVelocity = new Vector3(0, rigidB.angularVelocity.y, rigidB.angularVelocity.z);
            if (Input.GetAxis("Roll" + playerNum) != 0)
                rigidB.angularVelocity = new Vector3(rigidB.angularVelocity.x, rigidB.angularVelocity.y, 0);
            if (Input.GetAxis("Yaw" + playerNum) != 0)
                rigidB.angularVelocity = new Vector3(rigidB.angularVelocity.x, 0, rigidB.angularVelocity.z);
        }
        else
            rigidB.angularVelocity = Vector3.zero;
    }

    /*
    * This method controls the speed of the player's quadcopter rotars.
    */
    protected override void RotarAnimation()
    {
        // First, I calculate the thrust by taking the input axis and multiplying it by 1.6f.
        float currThrust = -Input.GetAxis("Thrust" + playerNum) * 1.6f;

        // Next I check to see if that thrust is a negative number.
        bool isMinus = false;
        if (currThrust < 0)
            isMinus = true;

        /*
         *  Next, I compare the current rotarSpeed to the thrust.
         *  
         *  If the thrust is more than the rotarSpeed, the rotarSpeed = the Thrust.
         *  (Same thing for a negative thrust only checks if lower.)
         *  
         *  Then while the player isn't inputing anything on the axis, I passively reduce the rotarSpeed over time.
         */
        if (currThrust != 0 && !isMinus && currThrust > rotarSpeed)
            rotarSpeed = currThrust;
        else if (currThrust != 0 && isMinus && currThrust < rotarSpeed)
            rotarSpeed = currThrust;
        else
        {
            if (rotarSpeed > 0.1f)
                rotarSpeed -= Time.deltaTime * 1.3f;
            else if (rotarSpeed < -0.1f)
                rotarSpeed += Time.deltaTime * 1.3f;
            else
                rotarSpeed = 0;
        }

        animator.SetFloat("RotarSpeed", rotarSpeed);
    }

    /*
     *  The CameraSwap method allows the player to switch into:
     *    > 1st person mode with the up-dpad/up-arrow
     *    > 3rd person mode with the down-dpad/down-arrow
     */
    private void CameraSwap()
    {
        if (Input.GetAxis("Camera" + playerNum) > 0)
        {
            camera1st.enabled = true;
            camera3rd.enabled = false;
        }
        else if (Input.GetAxis("Camera" + playerNum) < 0)
        {
            camera1st.enabled = false;
            camera3rd.enabled = true;
        }
    }

    /*
     * CalculateSignalStrength() calculates the distance the player is from the center of the map and updates the Wifi symbol on the HUD.
     */
    private void CalculateSignalStrength()
    {
        float distanceFromCentre = Vector3.Distance(transform.position, centreOfMap.transform.position);
        
        if (distanceFromCentre >= maxDistanceFromCenter)
            Die();
        else if (distanceFromCentre >= maxDistanceFromCenter * 0.75f)
            signalStrengthImage.sprite = wifi_Gone;
        else if (distanceFromCentre >= maxDistanceFromCenter * 0.5f)
            signalStrengthImage.sprite = wifi_Weak;
        else if (distanceFromCentre >= maxDistanceFromCenter * 0.25f)
            signalStrengthImage.sprite = wifi_Medium;
        else
            signalStrengthImage.sprite = wifi_Strong;
    }


    protected override void BoostTimer()
    {
        base.BoostTimer();
        if (isBoostActive && boostTime > 0)
            boostText.text = "Boost: " + boostTime;
        else if (!isBoostActive)
            boostText.text = "";
    }

    /*
     *  The OnTriggerStay() method inherits from Quadcopter.
     *  The only new functionality is updating the UI.
     */
    protected override void OnTriggerStay(Collider coll)
    {
        base.OnTriggerStay(coll);
        healthText.text = "Health: " + (int)health;
    }

    /*
     *  The OnTriggerExit() method inherits from Quadcopter.
     *  The only new functionality is updating the UI.
     */
    protected override void OnTriggerExit(Collider coll)
    {
        base.OnTriggerExit(coll);
        scoreText.text = "Score: " + score;
    }

    /*
     *  The OnCollisionEnter() method inherits from Quadcopter.
     *  The only new functionality is updating the UI.
     */
    protected override void OnCollisionEnter(Collision coll)
    {
        if (!isPlayerDead)
        {
            base.OnCollisionEnter(coll);
            healthText.text = "Health: " + (int)health;
        }
    }

    protected override void Die()
    {
        base.Die();

        if(health <= 0)
        {
            healthText.text = "0";
            animator.SetFloat("RotarSpeed", 0);
            rotarSounds.volume = 0;
            livesText.text = "0 lives";
        }
    }
}
