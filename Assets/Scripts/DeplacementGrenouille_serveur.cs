using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeplacementGrenouille_serveur : NetworkBehaviour
{
    [Header("Composants & Config")]
    [SerializeField] private float distanceSaut = 1.0f; // Distance de chaque bond (unité Unity)
    [SerializeField] private SpriteRenderer spriteRenderer; //Ref au renderer du sprite pour changer sa couleur
    [SerializeField] private Vector2 posDepartClient1; //Position de départ du client
    [SerializeField] private Vector2 posDepartClient2; //Position de départ du client
    [SerializeField] private Vector2 posDepartClient3; //Position de départ du client
    [SerializeField] private Vector2 posDepartServeur; //Position de départ de l'host
    private PlayerInput playerInput;
    // Synchronise la couleur sur tout le réseau
    [SerializeField] List<Color> listeCouleur =  new List<Color>();
    private NetworkVariable<Color> playerColor = new NetworkVariable<Color>(Color.white);


    /*
       - Mémorisation du spriterenderer si vide
       - Récupération du component playerInput
      */
    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        playerInput = GetComponent<PlayerInput>();
    }

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

        // Non nécessaire sur Mac. Sur PC, il faut s'assurer d'activer le component et de lui
        // attribuer le bon controScheme.
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

    // Exécutée lors du changement de la variable playerColor. Attribution de la couleur sur tous
    // les clients connectés.
    private void OnChangeCouleur(Color ancienneCouleur, Color nouvelleCouleur)
    {
        spriteRenderer.color = nouvelleCouleur;
    }


    /* Exécutée seulement sur le host (serveur)
     Permet l'attribution d'une couleur différente pour chaque grenouille
     */
    private void SetupPlayer()
    {
        // On détermine si c'est le joueur 1 (Host/Premier arrivé) ou le joueur 2
        // OwnerClientId == 0 est généralement le Host / premier joueur
        Debug.Log("OwnerClientID = " + OwnerClientId);
        playerColor.Value = listeCouleur[(int)OwnerClientId];
        if (OwnerClientId == 0)
        {
            transform.position = posDepartServeur;
        }
        else if (OwnerClientId == 1)
        {
            transform.position = posDepartClient1;
        }
        else if (OwnerClientId == 2)
        {
            transform.position = posDepartClient2;
        }
        else if (OwnerClientId == 3)
        {
            transform.position = posDepartClient3;
        }

        bool estPremierJoueur = OwnerClientId == 0;

        // 1. Positionnement côté serveur
        //transform.position = estPremierJoueur ? posDepartServeur : posDepartClient;

        // 2. Attribution de la couleur côté serveur (sera répliquée chez tout le monde)
        //playerColor.Value = estPremierJoueur ? Color.green : Color.red;
    }



    /*  
    Version authority = Server.
    Méthode appelée par le système d'Input lorsqu'une action de mouvement est détectée
    */
    public void OnMove(InputValue value)
    {
        // Seul le propriétaire de cette grenouille capte ses propres entrées clavier/manette
        if (!IsOwner) return;

        // Récupère la direction saisie (ZQSD / Flèches / D-Pad)
        // On ne traite que lorsqu'une touche vient d'être pressée
        Vector2 inputVector = value.Get<Vector2>();
        if (inputVector == Vector2.zero) return;

        // Déterminer la direction principale (pas de diagonale)
        Vector2 directionMouvement = Vector2.zero;

        // Vérifie si le mouvement horizontal est plus fort que le mouvement vertical
        if (Mathf.Abs(inputVector.x) > Mathf.Abs(inputVector.y))
        {
            // Force le déplacement uniquement sur l'axe X (-1 pour Gauche, 1 pour Droite) et annule l'axe Y
            directionMouvement = new Vector2(Mathf.Sign(inputVector.x), 0);
        }
        else
        {
            // Force le déplacement uniquement sur l'axe Y (-1 pour Bas, 1 pour Haut) et annule l'axe X
            directionMouvement = new Vector2(0, Mathf.Sign(inputVector.y));
        }

        // Demande au serveur d'exécuter le déplacement. Le serveur est le seul à pouvoir modifier le Transform de l'objet réseau.
        DeplacementGrenouille_Rpc(directionMouvement);
    }

    /*
    Méthode exécutée côté serveur uniquement pour déplacer la grenouille.
    */
    [Rpc(SendTo.Server)]
    private void DeplacementGrenouille_Rpc(Vector2 direction)
    {
        // Oriente le haut du sprite (Vector2.up) vers la direction souhaitée
        transform.rotation = Quaternion.FromToRotation(Vector2.up, direction);

        // 2. Application du déplacement (Le serveur modifie le Transform)
        transform.position += (Vector3)direction * distanceSaut;
    }

}