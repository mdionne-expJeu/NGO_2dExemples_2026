using UnityEngine;
using TMPro;
using Unity.Netcode; // namespace pour utiliser Netcode
using UnityEngine.SceneManagement; // namespace pour la gestion des scènes
using System;  // Nécessaire pour utiliser les Actions

public class GameManager : NetworkBehaviour //pour un network object
{
    public static GameManager instance;// Singleton pour parler au GameManager de n'importe où
    public bool partieEnCours { private set; get; } //permet de savoir si une partie est en cours
    public bool partieTerminee { private set; get; } // permet de savoir si une partie est terminée
    public Action OnDebutPartie; // Création d'une action auquel d'autres scripts pourront s'abonner.

    [SerializeField] GameObject panelConnection;
    [SerializeField] GameObject panelAttente;
    [SerializeField] GameObject boutonLancement;

    [SerializeField] TextMeshProUGUI texteAttente;
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
        panelAttente.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.Singleton.OnClientConnectedCallback += OnNouveauClientConnecte;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.Singleton.OnClientConnectedCallback -= OnNouveauClientConnecte;
    }

    /* Fonction qui sera appelée lors du callback OnClientConnectedCallback
   Gestion de l'affichage et du début de la partie en fonction du nombre de clients connectés.
   Si juste un client : c'est l'hôte... on affiche un panneau d'attente
   Si deux client : on lance la partie
   */
    private void OnNouveauClientConnecte(ulong obj)
    {
        panelAttente.SetActive(true);
        //if (!IsServer) return;
        Debug.Log("OnNouveauClientConnecté");
        if (NetworkManager.Singleton.ConnectedClients.Count == 1)
        {
            
        }
        else if (NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            texteAttente.text = "Il manque 2 joueurs pour commencer la partie";
        }
        else if (NetworkManager.Singleton.ConnectedClients.Count == 3)
        {
            texteAttente.text = "Il manque 1 joueurs pour commencer la partie";
        }
        else if (NetworkManager.Singleton.ConnectedClients.Count == 4)
        {
            texteAttente.text = "L'hôte du jeu peut lancer la partie";
            if (IsServer) boutonLancement.SetActive(true); 
        }
    }





    // L'hôte de la partie attend que 2 joueurs soient connectés pour lancer la partie
    // Seulement l'hôte exécute ce code
    // Aucune vérification si partie déjà en cours
    void Update()
    {
        if (!IsHost) return;
        if (partieEnCours) return;

        /*if (NetworkManager.Singleton.ConnectedClientsList.Count >= 2)
        {
            NouvellePartie();
            partieEnCours = true;
        }*/
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
    public void NouvellePartie()
    {
        // Déclenche l'action pour que les scripts abonnés puissent réagir au début de la partie
        OnDebutPartie?.Invoke();
    }

    // Fonction appelée par le ScoreManager pour terminer la partie (nous l'utilserons plus tard)
    public void FinPartie()
    {
        partieTerminee = true;
    }

}
