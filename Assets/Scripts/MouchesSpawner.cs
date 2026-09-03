using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class MouchesSpawner : NetworkBehaviour
{
    public static MouchesSpawner instance;
    [SerializeField] GameObject mouchePrefab; // Prefabs à instancier/spawner
    [SerializeField] int limitePosX = 8;
    [SerializeField] int limitePosY = 4;
    private Coroutine spawnBonus_Coroutine; // Référence à une coroutine

    [SerializeField] private List<Vector2Int> positionsOccupees = new List<Vector2Int>();

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
    - On désactive ce gameObject si on n'est pas le serveur
    - Abonnement à l'action OnDebutPartie du GameManager
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

    // Désabonnement à l'action OnDebutPartie du GameManager
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        GameManager.instance.OnDebutPartie -= OnDebutPartie;
    }

    /* Fonction qui sera appelée lorsque l'action OnDebutPartie du GameManager sera invoquée
    - On lance une coroutine qui va spawner des objets à une fréquence variable
    */
    private void OnDebutPartie()
    {

        if (!IsServer) gameObject.SetActive(false);
        Debug.Log("Appel coroutine");
        spawnBonus_Coroutine = StartCoroutine(GestionSpawn());
    }

    /* Coroutine qui instancie et spawn des objets à une fréquence variable
     - On instancie
     - On attribue une position aléatoire
     - On spawn l'objet pour qu'il apparaisse sur tous les clients
     */
    IEnumerator GestionSpawn()
    {
        while (true)
        {
            float attente = Random.Range(1f, 2f);
            yield return new WaitForSeconds(attente);

            int mouchePosX = ValeurPaireAlea(limitePosX);
            int mouchePosy = ValeurPaireAlea(limitePosY);
            Vector2Int nouvellePosition = new Vector2Int(mouchePosX, mouchePosy);

            if (positionsOccupees.Contains(nouvellePosition))
            {
                Debug.Log("Position " + nouvellePosition + " déjà prise ! On annule.");
            }
            else
            {
                GameObject nouvelleMouche = Instantiate(mouchePrefab);
                nouvelleMouche.transform.position = new Vector2(nouvellePosition.x, nouvellePosition.y);
                nouvelleMouche.GetComponent<NetworkObject>().Spawn();
                positionsOccupees.Add(nouvellePosition);
            }


        }
    }


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
}