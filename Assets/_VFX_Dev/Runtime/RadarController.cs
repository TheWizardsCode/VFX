using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RogueWave.VFX
{
    public class RadarController : MonoBehaviour
    {
        [Header("General Configuration")]
        [SerializeField, Layer, Tooltip("The tag of the GameObjects that the swarm will attack.")]
        int m_attackLayers = 0;
        [SerializeField, Tooltip("The detection radius of the radar, items within this radius will be shown on the radar.")]
        float m_scanningRadius = 10f;
        [SerializeField, Tooltip("The frequency of the radar scan.")]
        float m_scanFrequency = 1f;
        [SerializeField, Tooltip("The radius of the radar display.")]
        float m_displayRadius = 2f;

        [Header("Detection Blip Configuration")]
        [SerializeField, Tooltip("The parent object for the detection blips.")]
        Transform m_detectionBlipsParent;
        [SerializeField, Tooltip("The prototype object to spawn when an item is detected.")]
        GameObject m_detectionBlipPrototype;
        [SerializeField, Tooltip("The maxiumum number of objects that can be detected by the radar.")]
        int maxDetectedObjects = 10;

        float m_nextScanTime = 0f;
        private int layerMask;
        private Collider[] colliders;

        private void Awake()
        {
            layerMask = 1 << m_attackLayers;
            colliders = new Collider[maxDetectedObjects];
        }

        void Update()
        {
            m_nextScanTime -= Time.deltaTime;
            if (m_nextScanTime <= 0)
            {
                m_nextScanTime = m_scanFrequency;
                Scan();
            }
        }

        void Scan()
        {
            foreach (Transform child in m_detectionBlipsParent.transform)
            {
                Destroy(child.gameObject);
            }

            int count = Physics.OverlapSphereNonAlloc(transform.position, m_scanningRadius, colliders, layerMask);
            for (int i = 0; i < count; i++)
            {
                Vector3 directionToCollider = colliders[i].transform.position - transform.position;
                float distanceToCollider = directionToCollider.magnitude;
                float ratio = distanceToCollider / m_scanningRadius;

                Vector3 normalizedDirection = directionToCollider.normalized;
                Vector3 pos = transform.position + normalizedDirection * m_displayRadius * ratio;

                GameObject detectedObject = Instantiate(m_detectionBlipPrototype, pos, Quaternion.identity, m_detectionBlipsParent.transform);
                detectedObject.SetActive(true);

                LineRenderer lineRenderer = detectedObject.GetComponent<LineRenderer>();
                lineRenderer.SetPosition(0, detectedObject.transform.position);
                lineRenderer.SetPosition(1, new Vector3(detectedObject.transform.position.x, transform.position.y, detectedObject.transform.position.z));
            }
        }
    }
}