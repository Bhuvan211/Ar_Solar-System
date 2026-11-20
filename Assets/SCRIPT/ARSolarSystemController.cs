using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARSolarSystemController : MonoBehaviour
{
    [Header("Celestial Objects (Sun to Pluto)")]
    [SerializeField] private List<Transform> planetTargets = new List<Transform>();

    [Header("Tour Settings")]
    [SerializeField] private float pauseAfterAudio = 1.0f;
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private float distanceFromCamera = 0.6f;

    [Header("Rotation & Revolution Settings")]
    [SerializeField] private float selfRotationSpeed = 30f;
    [SerializeField] private float orbitSpeed = 10f;
    [SerializeField] private bool enableMotion = false;
    [SerializeField] private bool enableMoon = false;
    

    [Header("Audio Reference")]
    public AudioManager audioManager;

    [Header("Solar System, Earth, Moon ")]
    [SerializeField] private Transform moonObject;
    [SerializeField] private Transform earthObject;
    private Transform arCameraTransform;
    private Vector3 originalSystemPosition;
    private Quaternion originalSystemRotation;

    
    

    private void Start()
    {
        if (Camera.main != null)
            arCameraTransform = Camera.main.transform;

        originalSystemPosition = transform.position;
        originalSystemRotation = transform.rotation;

        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();

        StartCoroutine(FullTourSequence());
    }

    private void Update()
    {
        if (!enableMotion) return;

        
        foreach (Transform celestialObject in planetTargets)
        {
            if (celestialObject != null)
                celestialObject.Rotate(Vector3.up, selfRotationSpeed * Time.deltaTime, Space.Self);
        }

        
        if (planetTargets.Count > 0 && planetTargets[0] != null)
        {
            Vector3 centerPoint = planetTargets[0].position;

            for (int i = 1; i < planetTargets.Count; i++)
            {
                if (planetTargets[i] != null)
                {
                    float adjustedOrbitSpeed = orbitSpeed / Mathf.Sqrt(i);
                    planetTargets[i].RotateAround(centerPoint, Vector3.up, adjustedOrbitSpeed * Time.deltaTime);
                }
            }
        }
    }

    private IEnumerator FullTourSequence()
    {
        yield return new WaitForSeconds(1.5f);

        
        SetObjectsState(false);

        for (int i = 0; i < planetTargets.Count; i++)
        {
            Transform targetPlanet = planetTargets[i];

            yield return StartCoroutine(MoveObjectToCamera(targetPlanet));

            if (audioManager != null)
            {
                audioManager.PlayPlanetAudio(i);
                yield return StartCoroutine(WaitForAudioAndMaintainPosition(targetPlanet));
            }

            yield return StartCoroutine(PauseAndMaintainPosition(targetPlanet));
        }

       
        yield return StartCoroutine(ResetToOriginalPosition());
        enableMotion = true;

        if (audioManager != null)
        {
            
            yield return new WaitWhile(() => audioManager.IsPlaying());
        }

        yield return new WaitForSeconds(pauseAfterAudio);

        
        enableMotion = false;
        SetObjectsState(true);

        Transform focusTarget = GetFocusTarget();

        if (focusTarget != null)
        {
            yield return StartCoroutine(MoveObjectToCamera(focusTarget));

            if (audioManager != null)
            {
                

                
               
            }

            yield return StartCoroutine(PauseAndMaintainPosition(focusTarget));
        }

        enableMotion = true;
        yield return StartCoroutine(ResetToOriginalPosition());
    }    

    private void SetObjectsState(bool active)
    {
        enableMotion = active;
        enableMoon = active;
        

        if (moonObject != null) moonObject.gameObject.SetActive(active);
        
    }

    private Transform GetFocusTarget()
    {
        if (earthObject != null) return earthObject;
        if (planetTargets.Count > 3 && planetTargets[3] != null) return planetTargets[3];
        if (moonObject != null) return moonObject;
        
        return null;
    }

    private IEnumerator MoveObjectToCamera(Transform target)
    {
        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * transitionSpeed;
            Vector3 desiredPosition = GetCameraTargetPosition(target);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, elapsed / duration);
            yield return null;
        }
    }

    private void KeepObjectInFrontOfCamera(Transform target)
    {
        transform.position = GetCameraTargetPosition(target);
    }

    private Vector3 GetCameraTargetPosition(Transform target)
    {
        Vector3 targetCameraPosition = arCameraTransform.position + (arCameraTransform.forward * distanceFromCamera);
        Vector3 offset = target.position - transform.position;
        return targetCameraPosition - offset;
    }

    private IEnumerator WaitForAudioAndMaintainPosition(Transform target)
    {
        yield return null;
        while (audioManager.IsPlaying())
        {
            KeepObjectInFrontOfCamera(target);
            yield return null;
        }
    }

    private IEnumerator PauseAndMaintainPosition(Transform target)
    {
        float elapsed = 0f;
        while (elapsed < pauseAfterAudio)
        {
            elapsed += Time.deltaTime;
            KeepObjectInFrontOfCamera(target);
            yield return null;
        }
    }

    private IEnumerator ResetToOriginalPosition()
    {
        float elapsed = 0f;
        float duration = 2.0f;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * transitionSpeed;
            float t = elapsed / duration;

            transform.position = Vector3.Lerp(startPosition, originalSystemPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, originalSystemRotation, t);
            yield return null;
        }

        transform.position = originalSystemPosition;
        transform.rotation = originalSystemRotation;
    }
}