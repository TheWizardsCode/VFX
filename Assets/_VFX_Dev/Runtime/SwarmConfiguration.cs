using NaughtyAttributes;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// This class is used to configure the particle system.
/// 
/// When the system is created it will automatically configure the particle system
/// based according to the configuration set within. This component will then destory 
/// itself if configured to do so. However, if it remains active then it can be used to
/// adjust the particle system at runtime.
/// </summary>
namespace WizardsCode.VFX
{
    [ExecuteAlways]
    [RequireComponent(typeof(ParticleSystem))]
    public class SwarmConfiguration : MonoBehaviour
    {
        [Header("General Configuration")]
        [SerializeField, Layer, Tooltip("The tag of the GameObjects that the swarm will attack.")]
        int m_attackLayers = 0;
        [SerializeField, Tooltip("The detaection radius of the swarm, any enemies within this radius will be attacked.")]
        float m_detectionRadius = 1f;

        [Header("Movement")]
        [SerializeField, Tooltip("The speed at which each swarm will rotate around the base.")]
        float m_rotationalSpeed = 10f;
        [SerializeField, Tooltip("The radius of the circle the swarm will move around.")]
        float m_radius = 3f;
        
        [Header("Main Configuration")]
        [SerializeField, Tooltip("The minimum start size of the particle.")]
        float m_minStartSize = 0.05f;
        [SerializeField, Tooltip("The maximum start size of the particle.")]
        float m_maxStartSize = 0.1f;

        [Header("Shape Configuration")]
        [SerializeField, Tooltip("The radius of the swarm when in idle mode.")]
        float m_idleRadius = 1f;
        [SerializeField, Tooltip("The radius of the swarm when in attack mode.")]
        float m_attackRadius = 2f;

        [Header("Emission Configuration")]
        [SerializeField, Tooltip("The rate of particle emmision when in Idle mode.")]
        float m_idleEmissionRate = 5f;
        [SerializeField, Tooltip("The rate of particle emmision when in Idle mode.")]
        float m_attackEmissionRate = 50f;

        [Header("Velocity Over Lifetime Configuration")]
        [SerializeField, Tooltip("The minimum velocity of the particle over its lifetime.")]
        Vector3 m_minVelocityOverLifetime = new Vector3(0, 0f, 0);
        [SerializeField, Tooltip("The maximum velocity of the particle over its lifetime.")]
        Vector3 m_maxVelocityOverLifetime = new Vector3(0, 0.25f, 0);
        [SerializeField, Tooltip("The min/max radial speed of the particle when in idle mode.")]
        Vector2 m_idleRadialSpeed = new Vector2(0, 0);
        [SerializeField, Tooltip("The min/max radial speed of the particle when in swarm attack mode.")]
        Vector2 m_attackRadialSpeed = new Vector2(-5, -10);
        [SerializeField, Tooltip("The min/max radial speed of the particle when in swarm avoid mode.")]
        Vector2 m_avoidRadialSpeed = new Vector2(5, 10);

        [Header("Noise Configuration")]
        [SerializeField, Tooltip("Noise strength.")]
        float m_noiseStrength = 3f;
        [SerializeField, Tooltip("Noise frequency.")]
        float m_noiseFrequency = 0.5f;

        ParticleSystem particles;
        Collider[] colliders = new Collider[10];
        Vector3 swarmCenter = Vector3.zero;
        LayerMask layerMask;
        Transform target;
        float attackMoveSpeed = 3;

        internal float radius { 
            get { return m_radius; }
            set
            {
                m_radius = value;
            }
        }

        internal float rotationalSpeed
        {
            get { return m_rotationalSpeed; }
            set
            {
                m_rotationalSpeed = value;
            }
        }

        private void Awake()
        {
            particles = GetComponentInChildren<ParticleSystem>();
            if (particles == null)
            {
                Debug.LogError("No ParticleSystem component found on GameObject. This component will do nothing.");
                return;
            }

            swarmCenter = transform.parent.position;

            layerMask = 1 << m_attackLayers;

            Configure();
        }

        private void Update()
        {
            if (target == null)
            {
                transform.RotateAround(swarmCenter, Vector3.up, rotationalSpeed * Time.deltaTime);

                Vector3 direction = Vector3.zero;
                attackMoveSpeed = 1; // rotationalSpeed / 6;
                if (Vector3.Distance(transform.position, swarmCenter) > radius + 0.1f)
                {
                    direction = (swarmCenter - transform.position).normalized;
                } else if (Vector3.Distance(transform.position, swarmCenter) < radius - 0.1f)
                {
                    direction = (transform.position - swarmCenter).normalized;
                }
                transform.position = Vector3.MoveTowards(transform.position, transform.position + direction, attackMoveSpeed * Time.deltaTime);

                int count = Physics.OverlapSphereNonAlloc(transform.position, m_detectionRadius, colliders, layerMask);
                if (count > 0)
                {
                    target = colliders[0].transform;
                }
            }

            if (target != null)
            {
                if (target.gameObject.activeSelf == false)
                {
                    target = null;
                    SwarmIdle();
                }
                else
                {
                    SwarmAttack();
                    transform.position = Vector3.MoveTowards(transform.position, target.position, attackMoveSpeed * Time.deltaTime);
                }
            }
        }

        [Button]
        public void Configure()
        {
            GetComponent<SphereCollider>().radius = m_detectionRadius;  

            ConfigureMain(particles);
            ConfigureEmission(particles);
            ConfigureVelocityOverLifetime(particles);
            ConfigureNoise(particles);

            SwarmIdle();
            target = null;
        }

        [Button]
        public void SwarmAttack()
        {
            //OPTIMIZATION: Cache the shape and velocity modules
            ParticleSystem.ShapeModule shape = GetComponent<ParticleSystem>().shape;
            shape.radius = m_attackRadius;

            ParticleSystem.EmissionModule emission = GetComponent<ParticleSystem>().emission;
            emission.rateOverTime = m_attackEmissionRate;

            ParticleSystem.VelocityOverLifetimeModule velocity = GetComponent<ParticleSystem>().velocityOverLifetime;
            velocity.radial = new ParticleSystem.MinMaxCurve(m_attackRadialSpeed.x, m_attackRadialSpeed.y);
        }

        [Button]
        public void SwarmAvoid()
        {
            ParticleSystem.VelocityOverLifetimeModule velocity = GetComponent<ParticleSystem>().velocityOverLifetime;
            velocity.radial = new ParticleSystem.MinMaxCurve(m_avoidRadialSpeed.x, m_avoidRadialSpeed.y);
        }

        [Button]
        public void SwarmIdle()
        {
            //OPTIMIZATION: Cache the shape and velocity modules
            ParticleSystem.ShapeModule shape = GetComponent<ParticleSystem>().shape;
            shape.radius = m_idleRadius;

            ParticleSystem.EmissionModule emission = GetComponent<ParticleSystem>().emission;
            emission.rateOverTime = m_idleEmissionRate;

            ParticleSystem.VelocityOverLifetimeModule velocity = GetComponent<ParticleSystem>().velocityOverLifetime;
            velocity.radial = new ParticleSystem.MinMaxCurve(m_idleRadialSpeed.x, m_idleRadialSpeed.y);
        }

        private void ConfigureMain(ParticleSystem ps)
        {
            ParticleSystem.MainModule main = ps.main;
            main.startSize = new ParticleSystem.MinMaxCurve(m_minStartSize, m_maxStartSize);
        }

        private void ConfigureEmission(ParticleSystem ps)
        {
            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = m_idleEmissionRate;
        }

        private void ConfigureNoise(ParticleSystem ps)
        {
            ParticleSystem.NoiseModule noise = ps.noise;
            noise.strength = m_noiseStrength;
            noise.frequency = m_noiseFrequency;
        }

        private void ConfigureVelocityOverLifetime(ParticleSystem ps)
        {
            ParticleSystem.VelocityOverLifetimeModule velocity = ps.velocityOverLifetime;
            velocity.x = new ParticleSystem.MinMaxCurve(m_minVelocityOverLifetime.x, m_maxVelocityOverLifetime.x);
            velocity.y = new ParticleSystem.MinMaxCurve(m_minVelocityOverLifetime.y, m_maxVelocityOverLifetime.y);
            velocity.z = new ParticleSystem.MinMaxCurve(m_minVelocityOverLifetime.z, m_maxVelocityOverLifetime.z);
        }

    }
}