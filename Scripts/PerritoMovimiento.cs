using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class PerritoMovimiento : MonoBehaviour
{
    public Transform puntoObjeto; // 🟢 Asignalo desde el Inspector (el "anclaje" del hueso sobre la cabeza)

    [Header("Movimiento")]
    public float velocidad = 5f;
    public float aceleracion = 8f;
    public float desaceleracion = 8f;

    [Header("Salto")]
    public float fuerzaSalto = 12f;
    public float tiempoMaximoSalto = 0.5f;

    [Header("Salto en Pared")]
    public float fuerzaSaltoParedX = 8f;
    public float fuerzaSaltoParedY = 12f;
    public float velocidadDeslizamiento = -2f;
    public float gravedadExtra = 2f;

    public Rigidbody2D rb;
    private bool enSuelo;
    private bool enPared;
    private int direccionPared;
    private bool saltoEnParedActivo;
    private float tiempoSaltoActual;
    private int bonesCollected;

    private float inputHorizontal;
    private bool jumpPresionado;
    private bool jumpSostenido;
    private bool jumpLiberado;

    // Para recolección y robo
    private List<Collectible> objetosRecogidos = new List<Collectible>();
    public List<Collectible> ObjetosRecogidos => objetosRecogidos;

    [Header("Jugador")]
    public string jugadorID = "Jugador1"; // Este se usará como prefijo para Input

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        LeerInput();
        Mover();
        ManejarSalto();
        ControlDeslizamientoEnPared();
    }

    private void LeerInput()
    {
        inputHorizontal = Input.GetAxis(jugadorID + "_Horizontal");
        jumpPresionado = Input.GetButtonDown(jugadorID + "_Jump");
        jumpSostenido = Input.GetButton(jugadorID + "_Jump");
        jumpLiberado = Input.GetButtonUp(jugadorID + "_Jump");
    }

    private void Mover()
    {
        float velocidadObjetivo = inputHorizontal * velocidad;
        float suavizado = (inputHorizontal != 0) ? aceleracion : desaceleracion;

        rb.velocity = new Vector2(
            Mathf.Lerp(rb.velocity.x, velocidadObjetivo, suavizado * Time.deltaTime),
            rb.velocity.y
        );

        if (enSuelo && rb.velocity.y <= 0)
        {
            rb.gravityScale = 1f;
        }
    }

    private void ManejarSalto()
    {
        if (jumpPresionado)
        {
            if (enSuelo)
            {
                rb.velocity = new Vector2(rb.velocity.x, fuerzaSalto);
                tiempoSaltoActual = 0f;
            }
            else if (enPared)
            {
                SaltoEnPared();
            }
        }

        if (jumpSostenido && !enSuelo && !saltoEnParedActivo && !enPared)
        {
            if (tiempoSaltoActual < tiempoMaximoSalto)
            {
                tiempoSaltoActual += Time.deltaTime;
                float alturaSalto = Mathf.Lerp(fuerzaSalto, fuerzaSalto * 1.1f, tiempoSaltoActual / tiempoMaximoSalto);
                rb.velocity = new Vector2(rb.velocity.x, alturaSalto);
            }
        }

        if (jumpLiberado && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }

    private void ControlDeslizamientoEnPared()
    {
        if (enPared && !enSuelo && rb.velocity.y < 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, velocidadDeslizamiento);
        }
    }

    private void SaltoEnPared()
    {
        float direccionInput = inputHorizontal;
        float direccionSaltoX = (direccionInput != 0) ? -Mathf.Sign(direccionInput) : direccionPared;

        rb.velocity = new Vector2(direccionSaltoX * fuerzaSaltoParedX, fuerzaSaltoParedY);
        rb.gravityScale = gravedadExtra;
        saltoEnParedActivo = true;
        Invoke(nameof(ResetSaltoEnPared), 0.2f);
    }

    private void ResetSaltoEnPared()
    {
        saltoEnParedActivo = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contacto in collision.contacts)
        {
            if (contacto.normal.y > 0.5f)
                enSuelo = true;

            if (Mathf.Abs(contacto.normal.x) > 0.5f)
            {
                enPared = true;
                direccionPared = contacto.normal.x > 0 ? -1 : 1;
            }
        }

        PerritoMovimiento otro = collision.gameObject.GetComponent<PerritoMovimiento>();
        if (otro != null && otro.ObjetosRecogidos.Count > 0)
        {
            if (rb.velocity.magnitude > otro.rb.velocity.magnitude)
            {
                Collectible robado = otro.ObjetosRecogidos[0];
                otro.EliminarObjeto(robado);
                RecogerObjeto(robado);
                Debug.Log($"{name} le robó un objeto a {otro.name}");
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        enSuelo = false;
        enPared = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Perrito"))
        {
            PerritoMovimiento otroPerrito = other.GetComponent<PerritoMovimiento>();

            if (otroPerrito != null && otroPerrito.ObjetosRecogidos.Count > 0)
            {
                if (rb.velocity.magnitude > otroPerrito.rb.velocity.magnitude)
                {
                    Collectible robado = otroPerrito.ObjetosRecogidos[0];
                    otroPerrito.EliminarObjeto(robado);
                    RecogerObjeto(robado);
                    Debug.Log($"{name} le robó un objeto a {otroPerrito.name}");
                }
            }
        }

        if (other.CompareTag("Bone"))
        {
            bonesCollected++;
            Destroy(other.gameObject);
            Debug.Log("¡Hueso recogido! Total: " + bonesCollected);
        }

        Collectible objeto = other.GetComponent<Collectible>();
        if (objeto != null && objeto.portador == null)
        {
            RecogerObjeto(objeto);
            Debug.Log($"{name} recogió el objeto suelto");
        }
    }

    public void RecogerObjeto(Collectible objeto)
    {
        objetosRecogidos.Add(objeto);
        objeto.AsignarDueño(this);
    }

    public void EliminarObjeto(Collectible objeto)
    {
        if (objetosRecogidos.Contains(objeto))
        {
            objetosRecogidos.Remove(objeto);
            objeto.RemoverDueño();
        }
    }

    public void SoltarObjeto()
    {
        foreach (var objeto in objetosRecogidos)
        {
            objeto.transform.position = transform.position + Vector3.up;
            objeto.RemoverDueño();
        }

        objetosRecogidos.Clear();
    }

    public bool TieneObjeto()
    {
        return objetosRecogidos.Count > 0;
    }

    public int ObtenerTotalHuesos()
    {
        return bonesCollected;
    }
}
