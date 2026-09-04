using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeplacementGrenouille_serveur : NetworkBehaviour
{
    [Header("Composants & Config")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float jumpDistance = 1.0f;

    [Header("Positions de départ")]
    [SerializeField] private Vector3 posDepartServeur = new Vector3(-2, 0, 0);
    [SerializeField] private Vector3 posDepartClient = new Vector3(2, 0, 0);

    private PlayerInput playerInput;
    // Variable réseau pour synchroniser la couleur auprès de tous les clients
    private readonly NetworkVariable<Color> playerColor = new NetworkVariable<Color>(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        playerInput = GetComponent<PlayerInput>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Réaction au changement de couleur sur chaque client
        playerColor.OnValueChanged += OnColorChanged;
        spriteRenderer.color = playerColor.Value;

        // Seul le SERVEUR initialise la position et la couleur
        if (IsServer)
        {
            SetupPlayer();
        }
        gameObject.name = OwnerClientId.ToString();

        if (IsOwner)
        {
            // Active le composant et s'assure qu'il écoute les périphériques globaux
            if (playerInput != null)
            {
                playerInput.enabled = true;
                // Force l'Input System à associer le clavier/souris à ce PlayerInput sur le client
                playerInput.SwitchCurrentControlScheme(Keyboard.current);
            }
        }
        else
        {
            // Désactive les entrées pour les joueurs distants
            if (playerInput != null)
            {
                playerInput.enabled = false;
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        playerColor.OnValueChanged -= OnColorChanged;
    }

    private void SetupPlayer()
    {
        bool isFirstPlayer = OwnerClientId == 0;

        // Le serveur définit la position initiale et la couleur
        transform.position = isFirstPlayer ? posDepartServeur : posDepartClient;
        playerColor.Value = isFirstPlayer ? Color.green : Color.red;
    }

    private void OnColorChanged(Color previousValue, Color newValue)
    {
        spriteRenderer.color = newValue;
    }

    // --- GESTION DES DÉPLACEMENTS (Input System) ---

    public void OnMove(InputValue value)
    {
        Debug.Log($"OnMove appelé sur {gameObject.name} (NetworkId: {NetworkObjectId}) | IsOwner: {IsOwner}");
        Debug.Log("isOwner : " + IsOwner);
        // Seul le propriétaire de cette grenouille capte ses propres entrées clavier/manette
        if (!IsOwner) return;
        Debug.Log("move");
        Vector2 inputVector = value.Get<Vector2>();
        if (inputVector == Vector2.zero) return;

        // Déterminer la direction principale (pas de diagonal)
        Vector2 moveDirection = Vector2.zero;
        if (Mathf.Abs(inputVector.x) > Mathf.Abs(inputVector.y))
        {
            moveDirection = new Vector2(Mathf.Sign(inputVector.x), 0);
        }
        else
        {
            moveDirection = new Vector2(0, Mathf.Sign(inputVector.y));
        }
        Debug.Log("sendRPC");
        // Demande au serveur d'exécuter le saut
        MoveServerRpc(moveDirection);
    }

    [Rpc(SendTo.Server)]
    private void MoveServerRpc(Vector2 direction)
    {
        Debug.Log("On déplace la grenouille!");
        // 1. Calcul de la rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 2. Application du déplacement (Le serveur modifie le Transform)
        transform.position += (Vector3)direction * jumpDistance;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        //Debug.Log("Trigger détecté côté serveur !");

        if (collision.CompareTag("mouche"))
        {
            // Si la mouche est un NetworkObject, désaffichez-la ou despawnez-la via le réseau
            NetworkObject netObj = collision.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                //Debug.Log("despawn de la mouche");
                MouchesSpawner.instance.RetireListePos(collision.gameObject.transform.position);
                netObj.Despawn(); // Approche recommandée en réseau
            }
            else
            {
                collision.gameObject.SetActive(false);
            }
        }
    }
}