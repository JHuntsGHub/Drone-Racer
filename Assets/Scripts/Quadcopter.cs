using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * The Quadcopter class is the parent class for the Player class and the soon to come AIQuad class.
 * It holds all the common methods with functionality for damage taken, 
 */
public class Quadcopter : MonoBehaviour {

    protected const float speedForce = 1900f,  rotationSpeed = 90f, angularVelocityLimit = 4.5f, angularVelocityMinuser = 0.5f, boostLength = 10.0f;
    protected float health, rotarSpeed, boostTime;
    protected bool isBoostActive, isPlayerDead;

    protected int score { set; get; }

    /*
     *  The public (ie Dragged in from Unity) variables the Quadcopter class needs are as follows:
     *      > RigidBody  -->  For the movement and physics of the Quadcopter.
     *      > Animator  -->  For controlling the rotarSpeed.
     *      > ParticleSystem  -->  For the healing particles on the HealingPad.
     *      > Four GameObjects  -->  spareQuad1 & spareQuad2 for respawns in Die(), life1 & life2 for the UI displaying the spareQuads left.
     */
    public Rigidbody rigidB;
    public Animator animator;
    public ParticleSystem healingParticles;
    public AudioSource rotarSounds, crashSound, healingSound, boostSound;
    public ParticleSystem damageParticles;

    protected virtual void Start () {
        health = 100f;
        rotarSpeed = 0f;
        score = 0;
        isBoostActive = false;
        boostTime = 0;
        rigidB.maxAngularVelocity = angularVelocityLimit;
        healingParticles.enableEmission = false;
        isPlayerDead = false;
    }

    protected virtual void Update()
    {
        if (!isPlayerDead)
        {
            DoMovement();
            RotarAnimation();
            RotarVolume();
            BoostTimer();
        }
    }

    protected virtual void DoMovement()
    {
        //TODO - After AIQuad movement is done abstract the commonalities between that and the player for this method.
    }

    protected virtual void RotarAnimation()
    {
        //TODO - After AIQuad Animation is done abstract the commonalities between that and the player for this method.
    }

    /*
     * RotarVolume() controls the volume of the rotar sounds effect.
     */
    protected virtual void RotarVolume()
    {
        if (rotarSpeed <= 0.2f && rotarSpeed >= -0.2f)
            rotarSounds.volume = 0;
        else
        {
            if (rotarSpeed < 0)
                rotarSounds.volume = (-rotarSpeed / 10) * 3.5f;
            else
                rotarSounds.volume = (rotarSpeed / 10) * 3.5f;
        }
    }

    /*
     *  BoostTimer() simply reduces the time left on a boost.
     */
    protected virtual void BoostTimer()
    {
        if (isBoostActive)
        {
            boostTime -= Time.deltaTime;
            if (boostTime <= 0)
            {
                isBoostActive = false;
                boostTime = 0;
            }
        }
    }

    /*
     *  OnTriggerEnter() allows the healing particles to start emitting.
     */
    protected void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "HealingPad" && health < 100)
        {
            healingParticles.enableEmission = true;
            healingSound.Play();
        }
        else if(coll.tag == "Boost")
        {
            isBoostActive = true;
            boostTime += boostLength;
            boostSound.Play();
            Destroy(coll.gameObject);
        }
    }

    /*
     *  OnTriggerStay() Heals any Quadcopter that is in it. Stops particle emmissions once the Quadcopter reaches 100 health.
     */
    protected virtual void OnTriggerStay(Collider coll)
    {
        if (coll.tag == "HealingPad")
        {
            health += Time.deltaTime * 10;
            if (health > 100)
            {
                health = 100;
                healingParticles.enableEmission = false;
                healingSound.Stop();
            }
            else if(healingParticles.enableEmission == false || healingSound.isPlaying == false){
                healingParticles.enableEmission = true;
                healingSound.Play();
            }
        }
    }

    /*
     *  OnTriggerExit() Stops the healing particles from emitting.
     *  
     *  It also detects when a Quadcopter has passed through a target.
     *  When this is done it calls a method in the GameController class, passing in the target's gameObject - where the target is dissapeared and another appears.
     *  I also add to the player's score here:
     *      > 10 points are added to the score for every target passed through,
     *      > ***BUT*** The way Unity hantles OnTriggerExit() means that it is called for each Quadcopter collider that exits the Trigger.
     *      > This was a happy accident. Because of this:
     *              > if the player hits the center of the trigger, they get 30 points.
     *              > if the player hits the very edge of the trigger, they get 10 points.
     *              > if the player hits somewhere inbetween, they get 20 points.
     *      I decided this was a good thing as it rewards accuracy; so I kept it in.
     */
    protected virtual void OnTriggerExit(Collider coll)
    {
        if (coll.tag == "Target")
        {
            if(isBoostActive)
                score += 30;
            else
                score += 10;
            GameController.RingManager(coll.gameObject);
        }
        else if (coll.tag == "HealingPad")
        {
            healingParticles.enableEmission = false;
            healingSound.Stop();
        }
    }

    /*
     *  OnCollisionEnter() Calculates the damage the player takes from crashing (as long as cheats are off).
     *  
     *  The damage system is physics based and varies based on the Quadcopter's velocity.
     *  When the health reaches <= 0, Die() is called.
     */
    protected virtual void OnCollisionEnter(Collision coll)
    {
        if (!GameController.cheatsOn)
        {
            if (coll.gameObject.tag == "Target" || coll.gameObject.tag == "Obstacle" || coll.gameObject.tag == "HealingPad")
            {
                float damage = 0f;

                if (rigidB.velocity.x < 0)
                    damage += rigidB.velocity.x * -1;
                else
                    damage += rigidB.velocity.x;

                if (rigidB.velocity.y < 0)
                    damage += rigidB.velocity.y * -1;
                else
                    damage += rigidB.velocity.y;

                if (rigidB.velocity.z < 0)
                    damage += rigidB.velocity.z * -1;
                else
                    damage += rigidB.velocity.z;

                if (damage > 10)
                {
                    health -= damage / 2;
                    crashSound.volume = damage / 100;
                    crashSound.Play();

                    if (health > 0 && health < 1)
                        health = 1;
                }

                if (health <= 0)
                    Die();
            }
        }
    }

    /*
     *  Die() handles the death for the Quadcopter. 
     * 
     *  Currently, there is no pause - On death, the player is instantly teleported to one of their spareQuads. 
     */
    protected virtual void Die()
    {
        /*
         * The following is obsolete. It was for a three lives system.
         */
        //lives--;

        //if(lives <= 0)
        //{
        //    //TODO GameOver in single player, Do something else for multiplayer (Maybe a follow cam for another player?)
        //}
        //else
        //{
        //    // health, velocity, angularVelocity and RotarSpeed are all reset.
        //    health = 100;
        //    rigidB.velocity = Vector3.zero;
        //    rigidB.angularVelocity = Vector3.zero;
        //    rotarSpeed = 0;
        //    animator.SetFloat("RotarSpeed", 0);

        //    switch (lives)
        //    {
        //        // The player is teleported to a spareQuad and the UI's lives section is updated.
        //        case (1):
        //            transform.SetPositionAndRotation(spareQuad1.transform.position, spareQuad1.transform.rotation);
        //            Destroy(spareQuad1);
        //            Destroy(life1);
        //            break;
        //        case (2):
        //            transform.SetPositionAndRotation(spareQuad2.transform.position, spareQuad2.transform.rotation);
        //            Destroy(spareQuad2);
        //            Destroy(life2);
        //            break;
        //    }
        //}
        if (DataTransfer.MultiPlayer)
        {
            if(!isPlayerDead)
            {
                health = 0;
                isPlayerDead = true;
                DataTransfer.KillPlayer(gameObject.name, score);
            }
        }
        else
        {
            DataTransfer.Player1_Score = score;
            SceneManager.LoadScene("GameOver");
        }
    }
}
