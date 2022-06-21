using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    PhotonView pv;

    [SerializeField] Transform playerObj;
    [SerializeField] Transform camPos;
    PlayerShoot ps;

    [SerializeField] GameObject[] setYourPlayer;

    private Vector3 playerMovementInput;
    private Vector2 playerMouseInput;
    private float xRot;
    public Camera[] cameras;

    [Header("PlayerName")]
    public GameObject playerNameHolder;
    public Color teamNameColor, enemyNameColor;
    public float sphereCastRange;
    public float sphereCastThicness;
    public LayerMask playerNamesSphereCast, playerNamesLineCast;
    IEnumerator showCoroutine;

    [Header("Movement")]
    [SerializeField] private float speed;
    [SerializeField] private float crouchWalkSpeed;
    private Rigidbody rb;
    Vector3 moveVector;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask ground;

    [Header("Vaulting")]
    [SerializeField] float yOffset;
    [SerializeField] float yVaultCheck;
    [SerializeField] LayerMask vaultMask;

    [SerializeField] float vault1DistCheck, vault2DistCheck;

    [SerializeField] float vaultSpeed;
    [SerializeField] float vaultDistance;

    Vector3 currentVaultLoc;
    bool isVaulting;

    [Header("Crouching")]
    [SerializeField] float crouchSize;
    [SerializeField] float crouchOffset;
    [SerializeField] float crouchSpeed;

    [SerializeField] CapsuleCollider playerCollider;
    float colliderNormalSize;
    float playerNormalSize;
    float camNormalSize;

    bool isCrouching;

    [Header("Sprinting")]
    [SerializeField] private float sprintMultiplier = 1f;
    [HideInInspector] public bool isSprinting;
    private float currentSprintMultiplier = 1f;

    [Header("Camera")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float sensitivity;
    [HideInInspector] public float scopeSensMultiplier = 1;

    [Header("Recoil")]
    float recoil;
    float duration;
    float timer;

    [Header("Crosshair")]
    int crosshairSet;

    //[SerializeField] float resetTimer;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        ps = GetComponent<PlayerShoot>();

        colliderNormalSize = playerCollider.height;
        playerNormalSize = playerObj.localScale.y;
        camNormalSize = camPos.localPosition.y;

        if (PlayerInfoManager.perk3 != null && PlayerInfoManager.perk3.perkId == 0 && MultiplayerManager.hasPerks)
        {
            speed *= 1.2f;
            crouchWalkSpeed *= 1.2f;
        }
    }
    private void Start()
    {
        if (!pv.IsMine)
            return;

        cameras[0].tag = "MainCamera";
        pv.RPC("SetName", RpcTarget.Others, PhotonNetwork.NickName);
        foreach(Camera c in cameras)
        {
            c.enabled = true;
        }
        GetComponentInChildren<AudioListener>().enabled = true;

        foreach(GameObject g in setYourPlayer)
        {
            g.layer = 20;
        }

        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    [PunRPC]
    void SetName(string _name)
    {
        if(GetComponent<Health>().team == PhotonManager.team)
        {
            playerNameHolder.GetComponentInChildren<TMPro.TMP_Text>().text = _name;
            playerNameHolder.GetComponentInChildren<TMPro.TMP_Text>().color = teamNameColor;
            playerNameHolder.SetActive(true);
        }
        else
        {
            playerNameHolder.GetComponentInChildren<TMPro.TMP_Text>().text = _name;
            playerNameHolder.GetComponentInChildren<TMPro.TMP_Text>().color = enemyNameColor;
        }
    }
    private void Update()
    {
        if (!pv.IsMine)
            return;
        playerMovementInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        playerMovementInput = Vector3.ClampMagnitude(playerMovementInput, 1);
        if(!isCrouching)
            moveVector = transform.TransformDirection(playerMovementInput) * speed * currentSprintMultiplier;
        else
            moveVector = transform.TransformDirection(playerMovementInput) * crouchWalkSpeed * currentSprintMultiplier;
        playerMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        MovePlayer();
        MoveCamera();
        Crouching();
        Crosshair();
        CheckPlayerInView();

        if (Input.GetButtonDown("Right"))
        {
            ps.pm.DoScoreStreak();
        }
    }
    private void FixedUpdate()
    {
        if (!pv.IsMine)
            return;

        MovePlayerFixed();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y - 1, transform.position.z), .1f);
        if(isVaulting)
            Gizmos.DrawWireSphere(new Vector3(currentVaultLoc.x, currentVaultLoc.y + yOffset, currentVaultLoc.z), .5f);
    }
    void MovePlayer()
    {
        if (isVaulting)
            return;

        if (Input.GetButtonDown("Jump"))
        { 
            if (Physics.CheckSphere(new Vector3(transform.position.x, transform.position.y - 1, transform.position.z), .1f, ground))
            {
                bool vaultCheck = false;
                currentVaultLoc = transform.position + transform.forward * vaultDistance;
                if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z), transform.forward, vault1DistCheck, vaultMask))
                {
                    if(!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + yVaultCheck, transform.position.z), transform.forward, vault2DistCheck, vaultMask))
                    {
                        if(!Physics.CheckSphere(new Vector3(currentVaultLoc.x, currentVaultLoc.y + yOffset, currentVaultLoc.z), .5f, vaultMask))
                            vaultCheck = true;
                    }
                }
                if (vaultCheck)
                {
                    isVaulting = true;
                    StartCoroutine(Vault());
                }
                else
                    rb.AddForce(Vector3.up * (jumpForce - rb.velocity.y), ForceMode.Impulse);
            }
        }

        if (Input.GetButtonDown("Crouch"))
        {
            isCrouching = true;
        }
        else if (Input.GetButtonUp("Crouch"))
        {
            isCrouching = false;
        }

        if (!ps.aiming)
        {
            if (Input.GetButton("Run"))
                isSprinting = true;
            else if (Input.GetButtonUp("Run"))
                isSprinting = false;
        }

        if (isSprinting && playerMovementInput.z > 0)
            currentSprintMultiplier = sprintMultiplier;
        else
            currentSprintMultiplier = 1f;
    }
    IEnumerator Vault()
    {
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        gameObject.layer = 12;
        while(Vector3.Distance(transform.position, currentVaultLoc) > .1f)
        {
            transform.position = Vector3.Lerp(transform.position, currentVaultLoc, vaultSpeed * Time.deltaTime);
            yield return null;
        }
        gameObject.layer = 6;
        rb.useGravity = true;
        isVaulting = false;
    }
    void MovePlayerFixed()
    {
        if (isVaulting)
            return;

        rb.velocity = new Vector3(moveVector.x, rb.velocity.y, moveVector.z);
    }

    void MoveCamera()
    {
        float ownInput = playerMouseInput.y * sensitivity * scopeSensMultiplier * Time.deltaTime;
        xRot -= playerMouseInput.y * sensitivity * scopeSensMultiplier * Time.deltaTime;

        if (timer > 0)
        {
            xRot -= (recoil / duration * Time.deltaTime);
            timer -= Time.deltaTime;
        }
        //else if(resetTimer > 0.0001f)
        //{
        //    xRot += (recoil / duration * Time.deltaTime);
        //    resetTimer -= Time.deltaTime;
        //}
        //else if(resetTimer < 0.00001f)
        //{
        //    resetTimer = 0;
        //}

        xRot = Mathf.Clamp(xRot, -90, 90);

        transform.Rotate(0f, playerMouseInput.x * sensitivity * scopeSensMultiplier * Time.deltaTime, 0f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRot, 0, 0);
    }
    public void AddRecoil(float _recoil, float _duration)
    {
        recoil = _recoil;
        duration = _duration;
        timer += _duration;

        //resetTimer += _duration;
    }
    void Crouching()
    {
        if (isCrouching)
        {
            playerObj.localScale = Vector3.Lerp(playerObj.localScale, new Vector3(playerObj.localScale.x, playerNormalSize * crouchSize, playerObj.localScale.z), crouchSpeed * Time.deltaTime);
            playerCollider.height = Mathf.Lerp(playerCollider.height, colliderNormalSize * crouchSize, crouchSpeed * Time.deltaTime);
            camPos.localPosition = Vector3.Lerp(camPos.localPosition, new Vector3(camPos.localPosition.x, camNormalSize * crouchSize, camPos.localPosition.z), crouchSpeed * Time.deltaTime);
        }
        else
        {
            playerObj.localScale = Vector3.Lerp(playerObj.localScale, new Vector3(playerObj.localScale.x, playerNormalSize, playerObj.localScale.z), crouchSpeed * Time.deltaTime);
            playerCollider.height = Mathf.Lerp(playerCollider.height, colliderNormalSize, crouchSpeed * Time.deltaTime);
            camPos.localPosition = Vector3.Lerp(camPos.localPosition, new Vector3(camPos.localPosition.x, camNormalSize, camPos.localPosition.z), crouchSpeed * Time.deltaTime);
        }
    }
    void Crosshair()
    {
        if(moveVector == Vector3.zero)
        {
            if(crosshairSet != 0)
            {
                crosshairSet = 0;
                HudManager.Instance.SetCrosshairDist(crosshairSet);
            }
        }
        else if(!isSprinting)
        {
            if(crosshairSet != 1)
            {
                crosshairSet = 1;
                HudManager.Instance.SetCrosshairDist(crosshairSet);
            }
        }
        else
        {
            if(crosshairSet != 2)
            {
                crosshairSet = 2;
                HudManager.Instance.SetCrosshairDist(crosshairSet);
            }
        }
    }
    void CheckPlayerInView()
    {
        RaycastHit hit;
        if(Physics.SphereCast(playerCamera.position, sphereCastThicness, playerCamera.forward, out hit, sphereCastRange, playerNamesSphereCast))
        {
            if(hit.collider.tag == "Player")
                if(hit.collider.GetComponent<PlayerController>() != this)
                {
                    if(Physics.Linecast(playerCamera.position, hit.collider.transform.position, playerNamesLineCast))
                        hit.collider.GetComponent<PlayerController>().ShowName();
                }
        }
    }
    public void ShowName()
    {
        if (GetComponent<Health>().team == PhotonManager.team)
            return;

        if (showCoroutine != null)
            StopCoroutine(showCoroutine);
        showCoroutine = ShowNameCoroutine();
        StartCoroutine(showCoroutine);
    }
    IEnumerator ShowNameCoroutine()
    {
        playerNameHolder.SetActive(true);
        yield return new WaitForSeconds(1);
        playerNameHolder.SetActive(false);
    }
}
