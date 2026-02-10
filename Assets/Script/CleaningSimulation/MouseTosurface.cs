using UnityEngine;

public class MouseToSurface : MonoBehaviour
{
    public Rigidbody brushRigidbody;
    public LayerMask surfaceLayer;
    public float heightOffset = 0.05f;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // DISEGNA IL RAGGIO ROSSO PER DEBUG (Visibile nella finestra Scene)
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);

        if (Physics.Raycast(ray, out hit, 100f, surfaceLayer))
        {
            // Se colpisce, disegna una linea VERDE verso l'alto nel punto di impatto
            Debug.DrawRay(hit.point, Vector3.up, Color.green);

            Vector3 targetPosition = hit.point + (Vector3.up * heightOffset);
            
            if (brushRigidbody != null)
            {
                brushRigidbody.MovePosition(targetPosition);
            }
        }
    }
}