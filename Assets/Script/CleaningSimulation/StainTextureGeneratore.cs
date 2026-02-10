using UnityEngine;

public class StainSpawner : MonoBehaviour
{
    [Header("Configurazione")]
    public GameObject stainPrefab;    // Trascina qui il tuo oggetto 3D (es. una Sfera schiacciata)
    public Collider tableCollider;   // Il collider del tavolo
    public int amount = 10;          // Numero di pezzi di sporco

    void Start()
    {
        SpawnStains();
    }

    void SpawnStains()
    {
        Bounds b = tableCollider.bounds;

        for (int i = 0; i < amount; i++)
        {
            // 1. Calcola posizione casuale
            float x = Random.Range(b.min.x, b.max.x);
            float z = Random.Range(b.min.z, b.max.z);
            float y = b.max.y; // Appoggialo sulla superficie

            Vector3 pos = new Vector3(x, y, z);
            
            // 2. Rotazione casuale (rende lo sporco meno ripetitivo)
            Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 360), 0);

            // 3. Crea l'oggetto
            GameObject newStain = Instantiate(stainPrefab, pos, randomRot);

            // 4. Scala casuale (alcuni pezzi più grandi, altri più piccoli)
            float randomScale = Random.Range(0.05f, 0.2f); 
            newStain.transform.localScale = new Vector3(randomScale, randomScale * 0.5f, randomScale);
            
           
        }
    }
}