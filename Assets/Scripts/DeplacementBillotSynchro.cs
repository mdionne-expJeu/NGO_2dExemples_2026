using Unity.Netcode;
using UnityEngine;

public class DeplacementBillotSynchro : NetworkBehaviour
{
    [Header("Réglages du déplacement")]
    [SerializeField] private float speed = 2.0f;        // Vitesse du billot
    [SerializeField] private bool moveRight = true;     // Direction (Vrai = Droite, Faux = Gauche)

    [Header("Limites de la grille / écran")]
    [SerializeField] private float minX = -10.0f;       // Point d'apparition à gauche
    [SerializeField] private float maxX = 10.0f;        // Point de disparition à droite

    [Header("Décalage réseau")]
    [SerializeField] private float initialXOffset = 0f; // Position initiale sur la ligne

    private float totalDistance;

    private void Awake()
    {
        // Distance totale parcourue avant de boucler
        totalDistance = maxX - minX;
    }

    private void Update()
    {
        // On ne calcule le déplacement que si la session réseau est active

        //if (!IsSpawned) return;


        // 1. Récupération du temps officiel du serveur
        double serverTime = NetworkManager.Singleton.ServerTime.Time;
        // 2. Calcul de la distance totale parcourue depuis le début (Temps x Vitesse)
        float distanceTraveled = (float)(serverTime * speed);

        // 3. Application du rebouclage (Wrap-around) avec l'opérateur Modulo
        float currentOffset = (distanceTraveled + initialXOffset) % totalDistance;

        // 4. Positionnement selon la direction choisie
        float calculatedX;
        if (moveRight)
        {
            calculatedX = minX + currentOffset;
        }
        else
        {
            calculatedX = maxX - currentOffset;
        }

        // 5. Application de la position calculée
        transform.position = new Vector3(calculatedX, transform.position.y, transform.position.z);
    }
}