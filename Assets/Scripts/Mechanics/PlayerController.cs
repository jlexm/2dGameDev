using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

namespace Platformer.Mechanics
{
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        public float maxSpeed = 7;
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        public Collider2D collider2d;
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;

        public GameObject bulletPrefab1;
        public GameObject bulletPrefab2;
        public GameObject bulletPrefab3;
        public Transform firePoint;

        private GameObject currentBulletPrefab;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            // Set the initial bullet prefab
            currentBulletPrefab = bulletPrefab1;
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                move.x = Input.GetAxis("Horizontal");

                // Check for bullet selection input
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    // Switch to bulletPrefab1
                    currentBulletPrefab = bulletPrefab1;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    // Switch to bulletPrefab2
                    currentBulletPrefab = bulletPrefab2;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    // Switch to bulletPrefab3
                    currentBulletPrefab = bulletPrefab3;
                }

                // Check for shooting input
                if (Input.GetButtonDown("Fire1"))
                {
                    // Shoot the selected bullet
                    ShootBullet();
                }

                if (jumpState == JumpState.Grounded && Input.GetButtonDown("Jump"))
                    jumpState = JumpState.PrepareToJump;
                else if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else
            {
                move.x = 0;
            }

            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        void ShootBullet()
        {
            // Instantiate the selected bullet at the fire point position and rotation
            GameObject bullet = Instantiate(currentBulletPrefab, firePoint.position, Quaternion.identity);

            // Access the BulletController script and set its speed
            BulletController bulletController = bullet.GetComponent<BulletController>();
            if (bulletController != null)
            {
                // Set the bullet's speed
                bulletController.speed = 10f;

                // Set the bullet's direction based on player's facing direction
                float direction = spriteRenderer.flipX ? -1f : 1f;
                bulletController.SetDirection(direction);

            }
            else
            {
                Debug.LogError("BulletController component not found on the instantiated bullet.");
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}