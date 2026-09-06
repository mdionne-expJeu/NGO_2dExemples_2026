using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

/*
    Script pour gérer le spawn des mouches dans le jeu.
    Ce script doit être attaché à un GameObject vide dans la scène.
    Il est exécuté côté serveur uniquement pour gérer la logique de spawn.
*/

public class MouchesSpawner : NetworkBehaviour
{
    public static MouchesSpawner instance; // Singleton pour parler au MouchesSpawner de n'importe où
    [SerializeField] GameObject mouchePrefab; // Référence au prefab de la mouche à instancier
    [SerializeField] int limitePosX = 8; // Limite de position en X pour le spawn des mouches
    [SerializeField] int limitePosY = 4; // Limite de position en Y pour le spawn des mouches

    // Liste des positions actuellement occupées par les mouches pour éviter de spwaner si déjà 
    // une mouche à cette position
    [SerializeField] private List<Vector2> positionsOccupees = new List<Vector2>();

    // Coroutine pour gérer le spawn des mouches à intervalles aléatoires
    private Coroutine spawnBonus_Coroutine;

    //Création du singleton si nécessaire
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

    /*
    - Fonction appelée lorsque l'objet est spawné sur le réseau
    - Si ce n'est pas le serveur, on désactive le GameObject pour éviter que le client exécute 
    la logique de spawn.
    - On s'abonne à l'action OnDebutPartie du GameManager pour lancer le spawn des mouches
    */
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            gameObject.SetActive(false);
            return;
        }

        GameManager.instance.OnDebutPartie += OnDebutPartie;
    }

    // Désabonnement à l'action OnDebutPartie du GameManager lorsque l'objet est despawné
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        GameManager.instance.OnDebutPartie -= OnDebutPartie;
    }


    // Fonction appelée lorsque la partie commence. Lance la coroutine de spawn des mouches
    private void OnDebutPartie()
    {
        if (!IsServer) gameObject.SetActive(false);
        spawnBonus_Coroutine = StartCoroutine(GestionSpawn());
    }


    /* Coroutine qui gère le spawn des mouches à intervalles aléatoires
     - Génère une position aléatoire pour la mouche
     - Vérifie si la position est déjà occupée par une autre mouche
     - Si la position est libre, instancie la mouche et l'ajoute à la liste des positions occupées
     - Si la position est occupée, annule le spawn et attend le prochain intervalle
     */
    IEnumerator GestionSpawn()
    {
        while (true)
        {
            float attente = Random.Range(1f, 5f); // Intervalle aléatoire entre 1 et 5 secondes
            yield return new WaitForSeconds(attente); // Attente avant de spawn la prochaine mouche

            // Génération d'une position aléatoire pour la mouche
            int mouchePosX = ValeurPaireAlea(limitePosX);
            int mouchePosy = ValeurPaireAlea(limitePosY);
            Vector2 nouvellePosition = new Vector2(mouchePosX, mouchePosy);

            // Vérification si la position est déjà occupée par une autre mouche
            if (positionsOccupees.Contains(nouvellePosition))
            {
                Debug.Log("Position " + nouvellePosition + " déjà prise ! On annule.");
            }
            else
            {
                // Instanciation de la mouche
                GameObject nouvelleMouche = Instantiate(mouchePrefab);
                nouvelleMouche.transform.position = nouvellePosition;
                // Spawn la mouche sur le réseau pour qu'elle apparaisse chez tous les clients
                nouvelleMouche.GetComponent<NetworkObject>().Spawn();
                // Ajout de la position de la mouche à la liste des positions occupées
                positionsOccupees.Add(nouvellePosition);
            }


        }
    }

    /* Fonction qui génère une valeur aléatoire paire entre -limite et +limite
    - On divise la limite par 2 pour obtenir la moitié de la plage de valeurs possibles
    - On utilise Random.Range pour générer un entier aléatoire entre -moitieLimite et +moitieLimite
    - On multiplie le résultat par 2 pour obtenir une valeur paire
    */
    private int ValeurPaireAlea(int limite)
    {
        // 1. On divise la limite par 2 (ex: 8 devient 4)
        int moitieLimite = limite / 2;

        // 2. Random.Range avec des entiers exclut la borne supérieure, d'où le (+ 1)
        // Pour posX = 8 : génère un entier entre -4 et 4 inclus
        int posAlea = Random.Range(-moitieLimite, moitieLimite + 1);

        // 3. On remultiplie par 2 pour obtenir l'un des multiples de 2 (-8, -6, -4, ..., 8)
        return posAlea * 2;
    }

    /* Fonction publique pour retirer une position de la liste des positions occupées
    - Cette fonction est appelée par le script GrenouilleCollisions lorsqu'une mouche est attrapée 
      par une grenouille
    - On vérifie si la position est bien dans la liste avant de la retirer pour éviter les erreurs
    */
    public void RetireListePos(Vector2 posAretire)
    {
        if (positionsOccupees.Contains(posAretire))
        {
            positionsOccupees.Remove(posAretire);
        }
    }
}