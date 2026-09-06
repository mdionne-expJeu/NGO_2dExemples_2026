using UnityEngine;
using Unity.Netcode;

/*
    Script pour gérer les collisions de la grenouille avec les mouches.
    Ce script doit être attaché à l'objet (prefab) Grenouille.
    Il est exécuté côté serveur uniquement pour gérer la logique de collision.
   
    Important : la mouche doit avoir un tag "mouche" et un Collider2D avec "Is Trigger" activé 
    pour que la détection de collision fonctionne correctement.
*/
public class GrenouilleCollisions : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return; // Ce script ne doit être exécuté que sur le serveur


        if (collision.CompareTag("mouche"))
        {
            // Récupère le NetworkObject de la mouche pour pouvoir la despawner correctement
            NetworkObject netObj = collision.GetComponent<NetworkObject>();

            // Retire la position de la mouche de la liste des positions disponibles dans le spawner
            MouchesSpawner.instance.RetireListePos(collision.gameObject.transform.position);
            // Despawn la mouche côté serveur, ce qui la fera disparaître pour tous les clients
            netObj.Despawn();

        }
    }
}
