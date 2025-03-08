using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Slingshot : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject projectilePrefab;
    public float velocityMult = 10f;
    public Transform leftAnchor, rightAnchor; // Attach these in the Inspector

    [Header("Dynamic")]
    private GameObject launchPoint;
    private Vector3 launchPos;
    private GameObject projectile;
    private bool aimingMode;
    private bool isMouseOver = false;
    private Camera mainCam;
    private LineRenderer rubberBandLeft, rubberBandRight; // Rubber bands

    void Awake()
    {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        if (launchPointTrans == null)
        {
            Debug.LogError("LaunchPoint not found!");
            return;
        }

        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;
        mainCam = Camera.main;

        // Create and configure the rubber bands
        rubberBandLeft = CreateRubberBand("RubberBandLeft");
        rubberBandRight = CreateRubberBand("RubberBandRight");
    }

    void Update()
    {
        if (Mouse.current == null || mainCam == null) return;

        // Raycast to check if the mouse is over the slingshot
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (!isMouseOver)
                {
                    isMouseOver = true;
                    launchPoint.SetActive(true);
                }

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    StartAiming();
                }
            }
            else if (isMouseOver)
            {
                isMouseOver = false;
                launchPoint.SetActive(false);
            }
        }
        else if (isMouseOver)
        {
            isMouseOver = false;
            launchPoint.SetActive(false);
        }

        if (aimingMode)
        {
            AimAndShoot();
        }
    }

    private void StartAiming()
    {
        Debug.Log("Mouse Down - Aiming Started");
        aimingMode = true;
        projectile = Instantiate(projectilePrefab, launchPos, Quaternion.identity);
        Rigidbody projRB = projectile.GetComponent<Rigidbody>();
        projRB.isKinematic = true;

        rubberBandLeft.enabled = true;
        rubberBandRight.enabled = true;
    }

    private void AimAndShoot()
    {
        Vector3 mousePos2D = Mouse.current.position.ReadValue();
        mousePos2D.z = -mainCam.transform.position.z;
        Vector3 mousePos3D = mainCam.ScreenToWorldPoint(mousePos2D);
        Vector3 mouseDelta = mousePos3D - launchPos;

        float maxMagnitude = GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta = mouseDelta.normalized * maxMagnitude;
        }

        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;

        // Update rubber bands
        UpdateRubberBands(projPos);

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            LaunchProjectile(mouseDelta);
        }
    }

    private void LaunchProjectile(Vector3 direction)
    {
        aimingMode = false;
        Rigidbody projRB = projectile.GetComponent<Rigidbody>();
        projRB.isKinematic = false;
        projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
        projRB.linearVelocity = -direction * velocityMult;

        FollowCam.POI = projectile;
        projectile = null;

        // Disable rubber bands
        rubberBandLeft.enabled = false;
        rubberBandRight.enabled = false;
    }

    private LineRenderer CreateRubberBand(string name)
    {
        GameObject band = new GameObject(name);
        LineRenderer lr = band.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default")); // Simple material
        lr.startWidth = 0.08f;
        lr.endWidth = 0.1f;
        lr.positionCount = 2;
        lr.enabled = false;
        return lr;
    }

    private void UpdateRubberBands(Vector3 projPos)
    {
        if (leftAnchor && rightAnchor)
        {
            rubberBandLeft.SetPosition(0, leftAnchor.position);
            rubberBandLeft.SetPosition(1, projPos);

            rubberBandRight.SetPosition(0, rightAnchor.position);
            rubberBandRight.SetPosition(1, projPos);
        }
    }
}
