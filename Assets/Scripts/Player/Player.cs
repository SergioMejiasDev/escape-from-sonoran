using UnityEngine;

/// <summary>
/// Clase que permite moverse y atacar al jugador cuando va a pie (capítulos 1 y 2).
/// </summary>
public class Player : MonoBehaviour
{
    #region Variables

    /// <summary>
    /// Velocidad de movimiento del jugador.
    /// </summary>
    [Header("Movement")]
    [SerializeField] float speed = 4;
    /// <summary>
    /// Fuerza de salto del jugador.
    /// </summary>
    [SerializeField] float jump = 8.0f;
    /// <summary>
    /// Capa asignada al suelo.
    /// </summary>
    [SerializeField] LayerMask groundLayerMask = 0;
    /// <summary>
    /// Verdadero si el jugador está sobre una plataforma. Falso si no lo está.
    /// </summary>
    public bool inPlatform = false;

    /// <summary>
    /// Momento en el que se realizó el último disparo.
    /// </summary>
    [Header("Shoot")]
    float timeLastShoot;
    /// <summary>
    /// La cadencia de disparo.
    /// </summary>
    [SerializeField] float cadency = 0;
    /// <summary>
    /// El brazo del jugador.
    /// </summary>
    public GameObject arm;
    /// <summary>
    /// La posición donde se generan las balas.
    /// </summary>
    [SerializeField] Transform shootPoint = null;

    /// <summary>
    /// Componente Rigidbody2D del jugador.
    /// </summary>
    [Header("Components")]
    [SerializeField] Rigidbody2D rb = null;
    /// <summary>
    /// Componente Animator del jugador.
    /// </summary>
    [SerializeField] Animator anim = null;
    /// <summary>
    /// La cámara principal.
    /// </summary>
    Camera mainCamera;

    #endregion

    void Start()
    {
        mainCamera = Camera.main;
        arm.SetActive(true);
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");

        if (Time.timeScale != 0)
        {
            Movement(h);

            Animation(h);

            if (Input.GetButtonDown("Jump") && IsGrounded())
            {
                Jump();
            }

            if (Input.GetButton("Fire1") && Time.time - timeLastShoot > cadency)
            {
                Shoot();
            }

            if (Input.GetButtonDown("Cancel"))
            {
                GameManager.gameManager.PauseGame();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.parent = collision.transform;
            inPlatform = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            if (collision.gameObject.GetComponent<MovingPlatform>().isVertical && collision.gameObject.GetComponent<MovingPlatform>().direction == -1)
            {
                rb.gravityScale = 0;
            }

            else
            {
                rb.gravityScale = 1;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            inPlatform = false;
            transform.SetParent(null);
            rb.gravityScale = 1;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "SpawnTrigger1")
        {
            Level1Manager.level1Manager.SpawnZone();
            collision.gameObject.SetActive(false);
        }

        else if (collision.gameObject.name == "SpawnTrigger2")
        {
            Level2Manager.level2Manager.SpawnZone();
            collision.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Función que detecta a través de un raycast si el jugador está tocando el suelo.
    /// </summary>
    /// <returns>Verdadero si el jugador está en el suelo. Falso si no lo está.</returns>
    private bool IsGrounded()
    {
        RaycastHit2D hit1 = Physics2D.Raycast(new Vector2(transform.position.x - 0.8f, transform.position.y - 1.4f), Vector2.down, 0.2f, groundLayerMask);
        RaycastHit2D hit2 = Physics2D.Raycast(new Vector2(transform.position.x + 0.8f, transform.position.y - 1.4f), Vector2.down, 0.2f, groundLayerMask);
        
        return hit1 || hit2;
    }

    /// <summary>
    /// Función que permite el movimiento del jugador.
    /// </summary>
    /// <param name="h">La dirección de movimiento del jugador, que viene determinada por el input horizontal.</param>
    void Movement(float h)
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 cameraPosition = mainCamera.WorldToScreenPoint(transform.position);

        mousePos.x = mousePos.x - cameraPosition.x;
        mousePos.y = mousePos.y - cameraPosition.y;

        transform.Translate(Vector2.right * speed * Time.deltaTime * h);

        if (mousePos.x >= 0)
        {
            if (!inPlatform)
            {
                transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                arm.transform.localScale = new Vector3(1f, 1f, 1f);
            }

            else
            {
                transform.localScale = new Vector3(0.33f, 0.33f, 1f);
                arm.transform.localScale = new Vector3(1f, 1f, 1f);
            }

            float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
            arm.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        else if (mousePos.x < 0)
        {
            if (!inPlatform)
            {
                transform.localScale = new Vector3(-0.5f, 0.5f, 1f);
                arm.transform.localScale = new Vector3(-1f, -1f, 1f);
            }

            else
            {
                transform.localScale = new Vector3(-0.33f, 0.33f, 1f);
                arm.transform.localScale = new Vector3(-1f, -1f, 1f);
            }

            float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
            arm.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    /// <summary>
    /// Función que activa la animación del jugador.
    /// </summary>
    /// <param name="h">La dirección de movimiento del jugador, que viene determinada por el input horizontal.</param>
    void Animation(float h)
    {
        anim.SetBool("Walking", h != 0 && IsGrounded());
        anim.SetBool("Jumping", !IsGrounded());
    }

    /// <summary>
    /// Función que hace que el jugador salte.
    /// </summary>
    void Jump()
    {
        rb.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Función que hace que el jugador dispare.
    /// </summary>
    void Shoot()
    {
        GameObject bullet = ObjectPooler.SharedInstance.GetPooledObject("BulletPlayer");
        
        timeLastShoot = Time.time;
        
        if (bullet != null)
        {
            bullet.transform.position = shootPoint.position;
            bullet.transform.rotation = shootPoint.rotation;
            bullet.SetActive(true);
        }
    }
}