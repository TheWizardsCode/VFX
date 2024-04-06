using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.VFX
{
    public class SwarmCollectiveConfiguration : MonoBehaviour
    {
        [SerializeField, Tooltip("The prototype swarm configuration that will be used to create the individual swarms.")]
        SwarmConfiguration m_SwarmPrototype;
        [SerializeField, Tooltip("The number of individual swarms that make up the swarm collective.")]
        int numberOfSwarms = 1;

        [Header("Swarm Movement")]
        [SerializeField, Tooltip("The speed at which each swarm will rotate around the base.")]
        float m_RotationalSpeed = 10f;
        [SerializeField, Tooltip("The radius of the circle the swarm will move around.")]
        float m_radius = 3f;

        private void Awake()
        {
            float angleStep = 360f / numberOfSwarms;
            for (int i = 0; i < numberOfSwarms; i++)
            {
                float angle = angleStep * i;
                Vector3 position = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 1, Mathf.Sin(angle * Mathf.Deg2Rad)) * m_radius;
                
                SwarmConfiguration swarm = Instantiate(m_SwarmPrototype, transform);
                swarm.transform.position = position;
                swarm.radius = m_radius;
                swarm.rotationalSpeed = m_RotationalSpeed;
                swarm.gameObject.SetActive(true);
            }
        }
    }
}
