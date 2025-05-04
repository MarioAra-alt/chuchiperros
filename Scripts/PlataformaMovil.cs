using UnityEngine;

public class PlataformaMovil : MonoBehaviour
{
    public enum DireccionMovimiento { Horizontal, Vertical }
    public DireccionMovimiento direccion = DireccionMovimiento.Horizontal;

    public float distancia = 3f;         // Qué tanto se mueve desde el punto inicial
    public float velocidad = 2f;         // Velocidad del movimiento

    private Vector3 puntoInicial;
    private Vector3 direccionMovimiento;

    void Start()
    {
        puntoInicial = transform.position;
        direccionMovimiento = direccion == DireccionMovimiento.Horizontal ? Vector3.right : Vector3.up;
    }

    void Update()
    {
        float desplazamiento = Mathf.Sin(Time.time * velocidad) * distancia;
        transform.position = puntoInicial + direccionMovimiento * desplazamiento;
    }
}
