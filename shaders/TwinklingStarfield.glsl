// =====================================================
// Simple Twinkling Starfield Background
// =====================================================

// ===== FONCTION DE HASH (ALÉATOIRE) =====
// Génère un nombre pseudo-aléatoire entre 0.0 et 1.0 basé sur une position 2D.
float hash21(float2 p) {
    p = fract(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

// ===== COUCHE D'ÉTOILES =====
// Crée une couche d'étoiles à une échelle donnée.
// uv : coordonnées de l'écran
// scale : zoom de la grille (plus c'est grand, plus les étoiles sont petites et nombreuses)
// seed : décalage pour varier les couches
float starLayer(float2 uv, float scale, float seed) {
    
    // 1. Création d'une grille
    uv *= scale;
    uv += seed; // Décalage pour que les couches ne se superposent pas
    float2 gridID = floor(uv);  // Identifiant unique pour chaque case de la grille
    float2 gridUV = fract(uv) - 0.5; // Coordonnées locales dans la case (centrées)
    
    // 2. Valeur aléatoire unique par case
    float randomVal = hash21(gridID);
    
    // 3. Condition d'existence
    // On ne dessine une étoile que si la valeur aléatoire dépasse un seuil (ex: 0.9)
    // Cela rend le champ d'étoiles clairsemé.
    float star = 0.0;
    if (randomVal > 0.9) {
        
        // 4. Forme de l'étoile
        // Distance au centre de la case
        float d = length(gridUV);
        // Cercle très petit et doux. La taille varie légèrement avec randomVal.
        float shape = smoothstep(0.05 * randomVal + 0.01, 0.0, d);
        
        // 5. Scintillement (Twinkle)
        // Utilise le temps et la valeur aléatoire unique de l'étoile pour un scintillement déphasé.
        // sin(...) varie de -1 à 1. *0.5 + 0.5 le ramène de 0 à 1.
        float twinkle = sin(iTime * 3.0 + randomVal * 6.28318) * 0.5 + 0.5;
        
        // Combine forme, scintillement et luminosité de base
        star = shape * twinkle * randomVal;
    }
    
    return star;
}

// ===== MAIN =====
void mainImage(out float4 fragColor, in float2 fragCoord) {
    // Normalisation des coordonnées UV (-0.5 à 0.5)
    float2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    
    float3 col = float3(0.0, 0.0, 0.0);
    
    // 6. Composition de plusieurs couches
    // On superpose des couches de tailles différentes pour un effet de profondeur.
    
    // Couche 1 : Grandes étoiles proches, brillantes
    col += starLayer(uv, 10.0, 0.0) * 1.0; 
    
    // Couche 2 : Étoiles moyennes, plus nombreuses
    col += starLayer(uv, 25.0, 1.23) * 0.6;
    
    // Couche 3 : Petites étoiles lointaines, très nombreuses, moins brillantes
    col += starLayer(uv, 50.0, 4.56) * 0.3;
    
    // 7. Ajout d'un léger dégradé de fond (nébuleuse sombre)
    // Donne un ton bleuté très subtil vers le bas de l'écran
    col += float3(0.01, 0.02, 0.04) * (uv.y + 0.5);
    
    fragColor = float4(col, 1.0);
}