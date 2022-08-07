using UnityEngine;

/// <summary>
/// Clase que permite al jugador atacar y moverse cuando está volando (capítulo 3).
/// </summary>
public class FlyingPlayer : MonoBehaviour
{
    #region Variables

    /// <summary>
    /// La velocidad de movimiento del jugador.
    /// </summary>
    [Header("Movement")]
    [SerializeField] float speed = 4;

    /// <summary>
    /// Momento en el que se realizó el último disparo.
    /// </summary>
    [Header("Shoot")]
    float timeLastShoot;
    /// <summary>
    /// La cadencia de disparo del jugador.
    /// </summary>
    [SerializeField] float cadency = 0.25f;
    /// <summary>
    /// El brazo del jugador.
    /// </summary>
    public GameObject arm;
    /// <summary>
    /// La posición donde se generan las balas.
    /// </summary>
    [SerializeField] Transform shootPoint = null;

    /// <summary>
    /// La cámara principal.
    /// </summary>
    [Header("Components")]
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

        float v = Input.GetAxisRaw("Vertical");

        if (Time.timeScale != 0)
        {
            Movement(h, v);

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "SpawnTrigger3")
        {
            Level3Manager.level3Manager.StartFade();
            Destroy(collision.gameObject);
        }
    }

    /// <summary>
    /// Función que permite el movimiento del jugador.
    /// </summary>
    /// <param name="h">La dirección de movimiento en el eje X.</param>
    /// <param name="v">La dirección de movimiento en el eje Y.</param>
    void Movement(float h, float v)
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 cameraPosition = mainCamera.WorldToScreenPoint(transform.position);

        mousePos.x = mousePos.x - cameraPosition.x;
        mousePos.y = mousePos.y - cameraPosition.y;

        transform.Translate(Vector2.right * speed * Time.deltaTime * h);
        transform.Translate(Vector2.up * speed * Time.deltaTime * v);

        if (mousePos.x >= 0)
        {
            transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            arm.transform.localScale = new Vector3(1f, 1f, 1f);

            float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
            arm.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        else if (mousePos.x < 0)
        {
            transform.localScale = new Vector3(-0.5f, 0.5f, 1f);
            arm.transform.localScale = new Vector3(-1f, -1f, 1f);

            float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
            arm.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    /// <summary>
    /// Función que permite que el jugador dispare.
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