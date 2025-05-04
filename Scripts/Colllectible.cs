using UnityEngine;

public class Collectible : MonoBehaviour
{
    // Referencia al portador actual
    public Transform portador { get; private set; }
    private Transform puntoAnclaje;

    void Update()
    {
        if (portador != null && puntoAnclaje != null)
        {
            transform.position = puntoAnclaje.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger activado con: " + other.name);

        PerritoMovimiento perrito = other.GetComponent<PerritoMovimiento>();

        if (perrito != null && perrito != portador)
        {
            // Verificar si el perrito está en el suelo (usamos el flag `enSuelo` del perrito)
            if (perrito.TieneObjeto())
            {
                Debug.Log("¡Intento de robo!");
                // Comparamos las velocidades de los perritos
                PerritoMovimiento perritoActual = portador?.GetComponent<PerritoMovimiento>();

                if (perritoActual != null && perritoActual != perrito)
                {
                    // Verificamos cuál perrito es más rápido
                    if (perrito.rb.velocity.magnitude > perritoActual.rb.velocity.magnitude)
                    {
                        // Si el nuevo perrito es más rápido, robamos el objeto
                        Debug.Log("¡Robo realizado! El perrito más rápido se queda con el objeto.");
                        AsignarDueño(perrito);
                    }
                    else
                    {
                        // Si el perrito actual es más rápido, no ocurre el robo
                        Debug.Log("¡No hubo robo! El perrito actual es más rápido.");
                    }
                }
            }
        }
    }

    public void AsignarDueño(PerritoMovimiento perrito)
    {
        portador = perrito.transform; // Asignar el nuevo portador
        puntoAnclaje = perrito.puntoObjeto; // Posicionar el objeto en el punto de anclaje

        // Desactivar físicas y colisión
        GetComponent<Collider2D>().enabled = false;
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.simulated = false;
        }

        // Log para confirmar la asignación
        Debug.Log($"Objeto asignado a {perrito.name}");
    }

    public void RemoverDueño()
    {
        transform.SetParent(null); // Lo sacamos del perrito

        portador = null;
        puntoAnclaje = null;

        // Reactivar físicas y colisión si querés que caiga al suelo
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.velocity = Vector2.zero; // Reiniciar velocidad
        }

        Debug.Log($"{name} fue soltado");
    }
}
