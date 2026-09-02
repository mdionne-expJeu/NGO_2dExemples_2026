using UnityEngine;
using UnityEngine.InputSystem;

public class DeplacementGrenouille : MonoBehaviour
{
    [Header("Réglages du déplacement")]
    [SerializeField] private float jumpDistance = 1.0f; // Distance de chaque bond (unité Unity)

    public void OnMove(InputValue value)
    {
        // Récupère la direction saisie (ZQSD / Flèches / D-Pad)
        Vector2 inputVector = value.Get<Vector2>();

        // On ne traite que lorsqu'une touche vient d'être pressée
        if (inputVector == Vector2.zero) return;

        // Isoler l'axe principal pour éviter les déplacements diagonaux
        Vector2 moveDirection = Vector2.zero;

        if (Mathf.Abs(inputVector.x) > Mathf.Abs(inputVector.y))
        {
            moveDirection = new Vector2(Mathf.Sign(inputVector.x), 0);
        }
        else
        {
            moveDirection = new Vector2(0, Mathf.Sign(inputVector.y));
        }

        // 1. Orienter la grenouille vers la direction
        RotateTowards(moveDirection);

        // 2. Faire le bond d'un coup
        transform.position += (Vector3)moveDirection * jumpDistance;
    }

    private void RotateTowards(Vector2 direction)
    {
        // Calcule l'angle en degrés pour orienter le sprite (qui regarde vers le haut par défaut)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}