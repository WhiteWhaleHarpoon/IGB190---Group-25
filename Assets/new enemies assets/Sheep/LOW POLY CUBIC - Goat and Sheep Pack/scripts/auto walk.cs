using UnityEngine;
using UnityEngine.AI;

namespace Ursaanimation.CubicFarmAnimals
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class SheepAIController : MonoBehaviour
    {
        public string walkForwardAnimation = "walk_forward";
        public string idleAnimation = "idle"; // add an idle animation in your Animator
        public float wanderRadius = 10f;
        public float wanderTimer = 5f;

        private Animator animator;
        private NavMeshAgent agent;
        private float timer;

        void Start()
        {
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            timer = wanderTimer;
        }

        void Update()
        {
            timer += Time.deltaTime;

            // time to pick a new random spot
            if (timer >= wanderTimer)
            {
                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                agent.SetDestination(newPos);
                timer = 0;
            }

            // Play animations depending on movement
            if (agent.velocity.magnitude > 0.1f)
            {
                animator.Play(walkForwardAnimation);
            }
            else
            {
                animator.Play(idleAnimation);
            }
        }

        // Pick a random position on the NavMesh
        public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
        {
            Vector3 randDirection = Random.insideUnitSphere * dist;
            randDirection += origin;

            NavMeshHit navHit;
            NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

            return navHit.position;
        }
    }
}
