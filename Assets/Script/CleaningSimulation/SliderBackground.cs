
using UnityEngine;
using UnityEngine.UI;

public class SliderPainter : MonoBehaviour
{
    [Header("Collegamenti")]
    public Image backgroundImage; // Trascina qui l'oggetto 'Background' dello Slider

    [Header("Configurazione Zone (0-100)")]
    public float greenStart = 45f; // Dove inizia il verde (45°)
    public float greenEnd = 60f;   // Dove finisce il verde (60°)

    [Header("Colori")]
    public Color coldColor = Color.blue;
    public Color goodColor = Color.green;
    public Color hotColor = Color.red;

    void Start()
    {
        if (backgroundImage == null)
        {
            // Prova a trovarlo da solo se ti sei dimenticato
            backgroundImage = transform.Find("Background").GetComponent<Image>();
        }

        if (backgroundImage != null)
        {
            CreateTemperatureTexture();
        }
        else
        {
            Debug.LogError("Non trovo l'immagine di sfondo! Trascinala manualmente.");
        }
    }

    void CreateTemperatureTexture()
    {
        int width = 100; // Risoluzione orizzontale
        int height = 1;  // Altezza (basta 1 pixel, poi si stira)

        Texture2D texture = new Texture2D(width, height);
        
        // Loop per colorare ogni pixel
        for (int i = 0; i < width; i++)
        {
            Color pixelColor;

            if (i < greenStart)
            {
                // Zona Fredda (0 - 45)
                pixelColor = coldColor;
            }
            else if (i >= greenStart && i <= greenEnd)
            {
                // Zona Verde (45 - 60)
                pixelColor = goodColor;
            }
            else
            {
                // Zona Calda (60 - 100)
                pixelColor = hotColor;
            }

            texture.SetPixel(i, 0, pixelColor);
        }

        texture.Apply(); // Applica le modifiche

        // Crea uno Sprite dalla texture e assegnalo
        Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        backgroundImage.sprite = newSprite;
        backgroundImage.type = Image.Type.Simple; // Assicura che si stiri bene
    }
}