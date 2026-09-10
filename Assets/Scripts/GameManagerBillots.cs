using UnityEngine;
using TMPro;
using Unity.Netcode; // namespace pour utiliser Netcode
using UnityEngine.SceneManagement; // namespace pour la gestion des scènes
using System;  // Nécessaire pour utiliser les Actions

public class GameManagerBillots : NetworkBehaviour //pour un network object
{
    public static GameManagerBillots instance;// Singleton pour parler au GameManager de n'importe où
    public bool partieEnCours { private set; get; } //permet de savoir si une partie est en cours
    public bool partieTerminee { private set; get; } // permet de savoir si une partie est terminée
    public Action OnDebutPartie; // Création d'une action auquel d'autres scripts pourront s'abonner.

    [SerializeField] GameObject panelConnection;

    // Création du singleton si nécessaire
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        panelConnection.SetActive(true);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();


    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkSpawn();


    }

    /* Fonction qui sera appelée lors du callback OnClientConnectedCallback
   Gestion de l'affichage et du début de la partie en fonction du nombre de clients connectés.
   Si juste un client : c'est l'hôte... on affiche un panneau d'attente
   Si deux client : on lance la partie
   */






    // L'hôte de la partie attend que 2 joueurs soient connectés pour lancer la partie
    // Seulement l'hôte exécute ce code
    // Aucune vérification si partie déjà en cours
    void Update()
    {
        if (!IsHost) return;
        if (partieEnCours) return;

        if (NetworkManager.Singleton.ConnectedClientsList.Count >= 2)
        {
            //NouvellePartie();
            partieEnCours = true;
        }
    }


    // Fonction appelée pour le bouton qui permet de se connecter comme hôte
    public void LanceCommeHote() // Public pour être appeler de l'extérieur (par le bouton Hôte)
    {
        NetworkManager.Singleton.StartHost(); // Fonction du NetworkManager pour démarrer une partie comme hôte
    }

    // Fonction appelée pour le bouton qui permet de se connecter comme client
    public void LanceCommeClient() // Public pour être appeler de l'extérieur (par le bouton Client)
    {
        NetworkManager.Singleton.StartClient(); // Fonction du NetworkManager pour démarrer une partie comme client
    }

    // Activation d'une nouvelle partie lorsque 2 joueurs. 

    // Fonction appelée par le ScoreManager pour terminer la partie (nous l'utilserons plus tard)
    public void FinPartie()
    {
        partieTerminee = true;
    }

}
