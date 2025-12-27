using UnityEngine;
using UnityEngine.AI;
using System;

/// <summary>
/// ADVANCED ZOMBIE AI (FSM with Entry/Exit Pattern)
///
/// This script uses a Finite State Machine (FSM) driven by a critical SetState function
/// to manage all zombie behaviors, including:
/// 1. Tiered Detection (Irritation -> Lock-On -> Approach)
/// 2. Last Known Position (LKP) Investigation (NormalWalking state)
/// 3. Inter-Zombie Group Alerting (BroadcastAlert)
/// 4. Decoupled Combat Communication (TakeDamage)
/// 5. Entry/Exit Pattern: Crucially, it separates one-shot actions (like playing an animation)
///    from continuous logic (like calculating distance).
/// </summary>
public class AdvancedZombieAI : MonoBehaviour
{
    // ======================================================================
    // 1. STATE MANAGEMENT
    // ======================================================================
    /// <summary>
    /// Defines the possible behavioral states for the zombie.
    /// </summary>
    public enum State
    {
        IdleWalkSlow,   // State 1: Passive wandering (Low-cost state).
        NormalWalking,  // State 2: Investigation (Moving to Last Known Position/LKP).
        Running,        // State 3: Active pursuit with confirmed LOS.
        Approaching,    // State 4: Final, fast burst before attack range.
        Attacking,      // State 5: Stopped, performing damage/attack animation.
        Feeding,        // State 6: Reward state (placeholder).
        Dying           // State 7: Death animation/cleanup.
    }

    [Header("Zombie related Moves and sounds")]
    public State currentState;
    Animator zombieMoves;

    // CRITICAL: Tracks the state from the previous frame. Used by SetState() 
    // to trigger the one-shot setup logic in OnStateEnter().
    private State previousState;


    // ======================================================================
    // 2. DETECTION & COMBAT VARIABLES
    // ======================================================================
    [Header("Detection & Combat Ranges")]
    // Irritation: Uses inexpensive Raycast within FOV. Triggers NormalWalking.
    public float irritationRange = 30f;
    // Lock-On: Uses reliable SphereCast to confirm LOS. Triggers Running.
    public float pursueLockOnRange = 15f;
    // Approach: Inner range check. Triggers Approaching.
    public float approachSpeedRange = 5f;
    public float attackRange = 1f;
    // Radius for the SphereCast check. A volume-based check is more robust than a thin Raycast.
    public float sphereCastRadius = 0.5f;
    public float totalFOV = 60f;

    [Header("Layer Masks")]
    // Required to filter physics checks (SphereCast/Raycast) to only target the player.
    public LayerMask playerLayer;
    // Required to ensure walls and objects break Line of Sight (LOS).
    public LayerMask obstructionMask;

    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth; // Private variable to track current health


    // ======================================================================
    // 3. GROUP ALERT SETTINGS (Tiered Communication)
    // ======================================================================
    [Header("Group Alert Settings")]
    // Outer radius (e.g., 100m): Triggers investigation (NormalWalking). The "Whisper."
    public float investigationAlertRadius = 100f;
    // Inner radius (e.g., 20m): Triggers full pursuit (Running). The "Scream."
    public float pursuitAlertRadius = 20f;
    // Layer mask defining which objects are considered other zombies (for the alert system).
    public LayerMask zombieLayer;
    // Flag to ensure the zombie broadcasts its alert ONCE when entering the Running state.
    private bool alertBroadcasted = false;


    // ======================================================================
    // 4. MOVEMENT & WANDERING
    // ======================================================================
    [Header("Movement Speeds")]
    public float slowWalkSpeed = 2f;
    public float normalWalkSpeed = 4f;
    public float runSpeed = 6f;
    public float approachSpeed = 9.0f;
    public float deactivationTime = 10f;

    [Header("Wandering Settings")]
    public float wanderRadius = 10f;
    private float wanderTimer;
    public float minWanderInterval = 2f;
    public float maxWanderInterval = 5f;
    public float minStopTime = 2f;
    public float maxStopTime = 3f;
    private bool isWaiting = false;


    // ======================================================================
    // 5. PRIVATE REFERENCES & CORE STATE DATA
    // ======================================================================
    private NavMeshAgent agent;
    private Transform playerTransform;
    private float deathTimer=10f;
    // CRITICAL: Made PUBLIC so other zombie scripts (via BroadcastAlert) can modify 
    // this zombie's pursuit status directly.
    public bool isLockedOn = false;
    // private Animator animator; // Placeholder for animation component


    // ======================================================================
    // COMBAT AND DAMAGE COMMUNICATION (External API)
    // ======================================================================
    /// <summary>
    /// PUBLIC API: Called by an external object (like a Bullet script) on collision.
    /// This is the decoupled communication channel for combat.
    /// </summary>
    public void TakeDamage(float damage)
    {
        // Guard Clause: Don't process damage if the zombie is already dying.
        if (currentState == State.Dying) return;

        currentHealth -= damage;

        // Transition to dying if health is zero
        if (currentHealth <= 0)
        {
            // Use SetState to ensure death animations/cleanup runs ONCE.
            SetState(State.Dying);
        }
        // If hit while passive (Idle/NormalWalking), force immediate chase.
        else if (currentState != State.Running && currentState != State.Approaching && currentState != State.Attacking)
        {
            SetState(State.Running);
            isLockedOn = true;
            // Reset alert flag to broadcast the shooting to the group immediately.
            alertBroadcasted = false;
        }
    }


    // ======================================================================
    // CORE STATE MACHINE HANDLING
    // ======================================================================

    /// <summary>
    /// CRITICAL FUNCTION: Centralized transition handler for the FSM.
    /// This ensures all one-shot logic is executed exactly once per transition.
    /// </summary>
    private void SetState(State newState)
    {
        // 1. Guard check: Only transition if the state is actually changing.
        if (currentState != newState)
        {
            previousState = currentState;
            currentState = newState;

            // 2. The core of the Entry Pattern: trigger the one-shot setup.
            OnStateEnter();
        }
    }

    /// <summary>
    /// ENTRY POINT: Executes all one-shot setup logic when a new state begins.
    /// This is called ONLY once per transition by SetState().
    /// </summary>
    private void OnStateEnter()
    {
        // Use a switch case to handle the unique setup for each state.
        switch (currentState)
        {
            case State.IdleWalkSlow:
                // ONE-SHOT: Set speed (runs once).
                agent.speed = slowWalkSpeed;
                // animator.SetTrigger("IdleWalk"); // ONE-SHOT: Start idle animation.
                zombieMoves.SetTrigger("IsIdle");
                // CLEANUP: Reset flags from previous aggressive states.
                isLockedOn = false;
                alertBroadcasted = false;
                break;

            case State.NormalWalking:
                // ONE-SHOT: Set investigation speed.
                agent.speed = normalWalkSpeed;
                // animator.SetTrigger("Investigate"); 
                zombieMoves.SetTrigger("IsWalking");
                // LKP Logic: Set destination to player's position ONCE. 
                // This captures the LKP (Last Known Position) for investigation.
                if (playerTransform != null)
                {
                    agent.SetDestination(playerTransform.position);
                }
                break;

            case State.Running:
                // ONE-SHOT: Set full pursuit speed.
                agent.speed = runSpeed;
                // animator.SetTrigger("Run"); 
                zombieMoves.SetTrigger("IsRunning");
                // INTER-ZOMBIE COMMUNICATION: Broadcast the alert ONCE.
                BroadcastAlert();
                alertBroadcasted = true; // Flag to prevent re-broadcasting in Update().

                if (playerTransform != null)
                {
                    // CONTINUOUS LKP: This destination is continuously updated in RunningUpdate(),
                    // but we set it here for the immediate start.
                    agent.SetDestination(playerTransform.position);
                }
                break;

            case State.Approaching:
                // ONE-SHOT: Set fastest speed.
                agent.speed = approachSpeed;
                // animator.SetTrigger("ApproachFast");
                zombieMoves.SetTrigger("IsApproaching");
                break;
                 
            case State.Attacking:
                // ONE-SHOT: Stop movement immediately.
                agent.speed = 0;
                agent.SetDestination(transform.position);
                // animator.SetTrigger("AttackSwipe"); // Start attack animation.
                zombieMoves.SetTrigger("IsAttacking");
                break;

            case State.Dying:
                // ONE-SHOT: Disable agent for ragdoll/death animation purposes.
                if (agent.enabled) agent.enabled = false;
                deathTimer = deactivationTime;
                // animator.SetTrigger("Die");
                zombieMoves.SetTrigger("IsDying");
                break;
        }
    }

    // ======================================================================
    // HELPER METHODS (Communication & Pathfinding)
    // ======================================================================

    /// <summary>
    /// Handles inter-zombie communication using tiered alert radii.
    /// </summary>
    private void BroadcastAlert()
    {
        // Use the largest radius to find all potential targets.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, investigationAlertRadius, zombieLayer);
        Vector3 currentZombiePos = transform.position;

        foreach (var hitCollider in hitColliders)
        {
            // CRITICAL: Skip the zombie that initiated the alert.
            if (hitCollider.transform == this.transform) continue;

            // COMMUNICATION: Get a live reference to the other zombie's script.
            AdvancedZombieAI otherZombie = hitCollider.GetComponent<AdvancedZombieAI>();

            if (otherZombie != null)
            {
                // Only alert passive zombies (zombies already running don't need alerting).
                if ((otherZombie.currentState != State.IdleWalkSlow) && (otherZombie.currentState != State.NormalWalking)) continue;

                float distanceToOtherZombie = Vector3.Distance(currentZombiePos, hitCollider.transform.position);

                // Tier 1: NEAR Alert (Pursuit) - Instantly jumps to running.
                if (distanceToOtherZombie <= pursuitAlertRadius)
                {
                    otherZombie.SetState(State.Running);
                    otherZombie.isLockedOn = true;
                }
                // Tier 2: FAR Alert (Investigation) - Jumps to slow investigation.
                else if (distanceToOtherZombie <= investigationAlertRadius)
                {
                    otherZombie.SetState(State.NormalWalking);
                    // No lock-on: They must confirm LOS themselves when they get closer.
                    otherZombie.isLockedOn = false;
                }
            }
        }
    }

    /// <summary>
    /// Utility: Finds a safe, random point on the NavMesh for wandering.
    /// </summary>
    private Vector3 GetRandomNavMeshLocation(Vector3 origin, float radius)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
        randomDirection += origin;

        NavMeshHit hit;
        // NavMesh.SamplePosition finds the closest valid spot on the walkable mesh.
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return origin;
    }


    // ======================================================================
    // DETECTION LOGIC
    // ======================================================================

    /// <summary>
    /// Runs continuously to check for the player and determine the current threat level.
    /// </summary>
    /// <returns>0 (None), 1 (Irritation), 2 (Locked On), 3 (Approaching)</returns>
    int GetDetectionLevel()
    {
        if (playerTransform == null) return 0;

        Vector3 zombiePos = transform.position;
        Vector3 directionToPlayer = (playerTransform.position - zombiePos).normalized;
        float distanceToPlayer = Vector3.Distance(zombiePos, playerTransform.position);

        // CRITICAL FIX: Reset Lock-On every frame. If the player is behind cover, 
        // the checks below will fail, and 'isLockedOn' remains false, breaking pursuit.
        isLockedOn = false;

        // --- 3. Approaching Check (Level 3: Highest Priority) ---
        if (distanceToPlayer <= approachSpeedRange)
        {
            RaycastHit hit;
            // SphereCast is used for reliable LOS checking.
            if (Physics.SphereCast(zombiePos, sphereCastRadius, directionToPlayer, out hit, distanceToPlayer, playerLayer | obstructionMask))
            {
                if (((1 << hit.collider.gameObject.layer) & playerLayer) != 0)
                {
                    isLockedOn = true;
                    return 3;
                }
            }
        }

        // --- 2. Running/Lock-On Check (Level 2) ---
        if (distanceToPlayer <= pursueLockOnRange)
        {
            RaycastHit hit;
            if (Physics.SphereCast(zombiePos, sphereCastRadius, directionToPlayer, out hit, distanceToPlayer, playerLayer | obstructionMask))
            {
                if (((1 << hit.collider.gameObject.layer) & playerLayer) != 0)
                {
                    isLockedOn = true;
                    return 2;
                }
            }
        }

        // --- 1. Irritation Check (Level 1: Lowest Priority) ---
        if (distanceToPlayer <= irritationRange)
        {
            float halfFOV = totalFOV / 2f;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer <= halfFOV)
            {
                RaycastHit hit;
                // Simple Raycast is used for the lowest level of detection.
                if (Physics.Raycast(zombiePos, directionToPlayer, out hit, irritationRange, playerLayer | obstructionMask))
                {
                    if (((1 << hit.collider.gameObject.layer) & playerLayer) != 0)
                    {
                        return 1;
                    }
                }
            }
        }

        return 0; // Nothing detected
    }

    // ======================================================================
    // STATE IMPLEMENTATIONS (Continuous Update Logic)
    // ======================================================================

    /// <summary>
    /// Continuous logic for IdleWalkSlow state. Handles wandering and transition checks.
    /// </summary>
    void IdleWalkSlowUpdate()
    {
        // Check for transition out of idle.
        int detection = GetDetectionLevel();
        if (detection >= 1)
        {
            SetState(State.NormalWalking);
            return;
        }

        // --- Continuous Wandering Logic ---
        // A. Check if the zombie has reached its current target (Stopping Check)
        if (!isWaiting && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isWaiting = true;
            wanderTimer = UnityEngine.Random.Range(minStopTime, maxStopTime);
            agent.SetDestination(transform.position); // Command to stop
            return;
        }

        // B. If currently waiting/stopped (Timer Check)
        if (isWaiting)
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0)
            {
                // Waiting is over, find a new path.
                isWaiting = false;
                wanderTimer = UnityEngine.Random.Range(minWanderInterval, maxWanderInterval);
                Vector3 newPos = GetRandomNavMeshLocation(transform.position, wanderRadius);
                agent.SetDestination(newPos);
            }
        }
    }

    /// <summary>
    /// Continuous logic for NormalWalking (LKP Investigation) state.
    /// </summary>
    void NormalWalkingUpdate()
    {
        // CONTINUOUS: Agent keeps walking towards the destination set in OnStateEnter (the LKP).
        agent.SetDestination(playerTransform.position);

        int detection = GetDetectionLevel();

        // If LOS is confirmed (Level 2), transition to full pursuit.
        if (detection >= 2)
        {
            SetState(State.Running);
        }
        // If detection is completely lost (Level 0), or LKP is reached, drop back to idle.
        else if (detection == 0)
        {
            SetState(State.IdleWalkSlow);
        }
    }

    /// <summary>
    /// Continuous logic for Running (Active Pursuit) state.
    /// </summary>
    void RunningUpdate()
    {
        // CONTINUOUS: Must update destination every frame to track the moving player.
        agent.SetDestination(playerTransform.position);
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        int detection = GetDetectionLevel();

        if (distanceToPlayer <= attackRange)
        {
            SetState(State.Attacking);
        }
        else if (detection >= 3)
        {
            SetState(State.Approaching);
        }
        // If detection is 0, the player has escaped sight/LOS is broken.
        else if (detection == 0)
        {
            SetState(State.IdleWalkSlow);
        }
    }

    /// <summary>
    /// Continuous logic for Approaching (Final Rush) state.
    /// </summary>
    void ApproachingUpdate()
    {
        agent.SetDestination(playerTransform.position);
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            SetState(State.Attacking);
        }
        // If player backs up, drop back to the standard running speed/state.
        else if (distanceToPlayer > approachSpeedRange * 1.5f)
        {
            SetState(State.Running);
        }
        else if (GetDetectionLevel() == 0)
        {
            SetState(State.IdleWalkSlow);
        }
    }

    /// <summary>
    /// Continuous logic for Attacking state.
    /// </summary>
    void AttackingUpdate()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // If player runs out of attack range, return to running pursuit.
        if (distanceToPlayer > (attackRange * 1))
        {
            SetState(State.Running);
            return;
        }

        // ** CONTINUOUS: Attack Cooldown Checks, Damage Infliction Logic, etc. **
    }

    /// <summary>
    /// Continuous logic for Dying state. Handles timer and destruction.
    /// </summary>
    void DyingUpdate()
    {
        // CONTINUOUS: Decrement timer until destruction.
        deathTimer -= Time.deltaTime;

        if (deathTimer <= 0)
        {
            // FINAL CLEANUP: Remove object from the scene.
            Destroy(gameObject, deactivationTime);
        }
    }


    // ======================================================================
    // START & UPDATE (Initialization)
    // ======================================================================
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        zombieMoves = GetComponent<Animator>();
        agent.stoppingDistance = attackRange * 0.25f;
        currentHealth = maxHealth;

        // Initial setup for the player reference.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Use SetState to enter the initial state and trigger the first OnStateEnter.
        SetState(State.IdleWalkSlow);
    }

    /// <summary>
    /// Unity's main loop. Runs every frame and routes execution to the continuous
    /// update function for the current state.
    /// </summary>
    void Update()
    {
        // The main loop routes control to the current state's "Update" function

        //animator moving speed updator, for instant update of current zombie moving speed state
        //-keep going Amr you can fix this, your team depend on you at the final moments, your team deserve to try to the last moments for!, keep rolling-
        zombieMoves.SetFloat("Speed", agent.speed);

        // Main loop for machine state of the moving zombie, so this is the main logic for loopable action of the zombie like tracking, attaking, feeding, gitting hit or dmage or die.
        switch (currentState)
        {
            case State.IdleWalkSlow:
                IdleWalkSlowUpdate();
                break;
            case State.NormalWalking:
                NormalWalkingUpdate();
                break;
            case State.Running:
                RunningUpdate();
                break;
            case State.Approaching:
                ApproachingUpdate();
                break;
            case State.Attacking:
                AttackingUpdate();
                break;
            case State.Dying:
                DyingUpdate();
                break;
                // Add case for State.Feeding if implemented
        }
    }


}