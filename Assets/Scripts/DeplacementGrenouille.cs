using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class DeplacementGrenouille : NetworkBehaviour
{
    [Header("Réglages du déplacement")]
    [SerializeField] private float jumpDistance = 1.0f; // Distance de chaque bond (unité Unity)
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Vector2 posDepartClient;
    [SerializeField] private Vector2 posDepartServeur;

    // Synchronise la couleur sur tout le réseau(lecture pour tous, écriture serveur uniquement)
    private NetworkVariable<Color> playerColor = new NetworkVariable<Color>(Color.white);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Écouter les changements de couleur sur tous les clients
        playerColor.OnValueChanged += OnChangeCouleur;

        // Appliquer la couleur actuelle lors du spawn
        spriteRenderer.color = playerColor.Value;

        // Seul le SERVEUR gère le placement initial et l'attribution des couleurs
        if (IsServer)
        {
            SetupPlayer();
        }
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        playerColor.OnValueChanged -= OnChangeCouleur;

        // Appliquer la couleur actuelle lors du spawn
        spriteRenderer.color = playerColor.Value;
    }

    private void OnChangeCouleur(Color ancienneCouleur, Color nouvelleCouleur)
    {
        spriteRenderer.color = nouvelleCouleur;
    }

    private void SetupPlayer()
    {
        // On détermine si c'est le joueur 1 (Host/Premier arrivé) ou le joueur 2
        // OwnerClientId == 0 est généralement le Host / premier joueur
        bool isFirstPlayer = OwnerClientId == 0;

        // 1. Positionnement côté serveur
        transform.position = isFirstPlayer ? posDepartServeur : posDepartClient;

        // 2. Attribution de la couleur côté serveur (sera répliquée chez tout le monde)
        playerColor.Value = isFirstPlayer ? Color.green : Color.red;
    }


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