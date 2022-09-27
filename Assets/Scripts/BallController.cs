using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    Rigidbody rb;

    public float speed = 15;

    private bool isTravelling;
    private Vector3 travelDirection;
    private Vector3 nextCollisionPosition;

    public int minSwipeRecognition = 500;
    private Vector2 swipePosLastFrame;
    private Vector2 swipePosCurrrentFrame;
    private Vector2 currentSwipe;

    private Color solveColor;

    public ParticleSystem hitEffectParticle;
    public AudioSource hitSound;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        solveColor = Random.ColorHSV(0.5f, 1);
        GetComponent<MeshRenderer>().material.color = solveColor;

        if(hitEffectParticle == null)
        {
            hitEffectParticle = GameObject.Find("hitEffect").GetComponent<ParticleSystem>();
        }
        
    }

    private void FixedUpdate()
    {
        if(isTravelling)
        {
            rb.velocity = speed * travelDirection;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position - (Vector3.up / 2), 0.05f);
        int i = 0;

        while(i < hitColliders.Length)
        {
            GroundPiece ground = hitColliders[i].transform.GetComponent<GroundPiece>();
            if(ground && !ground.isColored)
            {
                ground.ChangeColor(solveColor);
                hitSound.Play();
                
                ParticleSystem.MainModule main = hitEffectParticle.main;
                main.startColor = solveColor; // <- or whatever color you want to assign

                hitEffectParticle.Play();
                Debug.Log(hitEffectParticle.isPlaying);
            }

            
            i++;
        }

        if(nextCollisionPosition != Vector3.zero)
        {
            if(Vector3.Distance(transform.position, nextCollisionPosition) < 1)
            {
                isTravelling = false;
                travelDirection = Vector3.zero;
                nextCollisionPosition = Vector3.zero;
            }
        }

        if (isTravelling)
            return;

        if(Input.GetMouseButton(0))
        {
            swipePosCurrrentFrame = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            if(swipePosLastFrame != Vector2.zero)
            {
                currentSwipe = swipePosCurrrentFrame - swipePosLastFrame;

                if(currentSwipe.sqrMagnitude < minSwipeRecognition)
                {
                    return;
                }

                currentSwipe.Normalize();

                //Up...Down
                if(currentSwipe.x > -0.5f && currentSwipe.x < 0.5)
                {
                    //go up/down
                    SetDestination(currentSwipe.y > 0 ? Vector3.forward : Vector3.back);
                }

                //Left...Right
                if (currentSwipe.y > -0.5f && currentSwipe.y < 0.5)
                {
                    //go Left/Right
                    SetDestination(currentSwipe.x > 0 ? Vector3.right : Vector3.left);
                }
            }

            swipePosLastFrame = swipePosCurrrentFrame;
        }

        //releasing the screen
        if(Input.GetMouseButtonUp(0))
        {
            swipePosLastFrame = Vector2.zero;
            currentSwipe = Vector2.zero;
        }
    }

    private void SetDestination(Vector3 direction)
    {
        travelDirection = direction;

        RaycastHit hit;

        if(Physics.Raycast(transform.position, direction, out hit, 100f))
        {
            nextCollisionPosition = hit.point;
        }

        isTravelling = true;
    }
}
