using UnityEngine;

namespace Sherlock.UI
{
    /// <summary>
    /// PlaceholderSpriteGenerator — erstellt hübsche Platzhalter-Icons aus Code.
    ///
    /// Wird verwendet solange noch keine echten Grafiken vorhanden sind.
    /// Jedes Item bekommt eine einzigartige Farbe + Symbol basierend auf seinem Namen.
    /// Kann jederzeit durch echte PNG-Dateien ersetzt werden.
    ///
    /// Die generierten Sprites sehen so aus:
    ///   • Runder farbiger Hintergrund (Tier-Farbe)
    ///   • Einfaches Symbol in der Mitte (je nach Kategorie)
    ///   • Goldener Rahmen bei Tier 4+
    /// </summary>
    public static class PlaceholderSpriteGenerator
    {
        private const int Size = 128; // Pixel

        // ── Item-Sprite ───────────────────────────────────────────────────────

        public static Sprite GenerateItemSprite(string itemId, int tier = 1)
        {
            var tex    = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            var pixels = new Color[Size * Size];

            var bgColor     = TierColor(tier);
            var borderColor = tier >= 4 ? new Color(1f, 0.85f, 0.2f) : new Color(0.2f, 0.15f, 0.1f);
            var iconColor   = Color.white;

            int center = Size / 2;
            int radius  = Size / 2 - 4;
            int border  = 6;

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float dx   = x - center;
                    float dy   = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > radius)
                        pixels[y * Size + x] = Color.clear;
                    else if (dist > radius - border)
                        pixels[y * Size + x] = borderColor;
                    else
                        pixels[y * Size + x] = bgColor;
                }
            }

            // Symbol zeichnen
            DrawIcon(pixels, itemId, center, iconColor);

            // Glanz-Effekt oben links
            DrawGloss(pixels, center);

            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            return SpriteManager.Texture2DToSprite(tex);
        }

        // ── Hintergrund-Placeholder (für HO-Szenen) ───────────────────────────

        public static Sprite GenerateSceneBackground(string sceneId, int width = 1080, int height = 1920)
        {
            var tex    = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color[width * height];

            // Dunkler Papier-Ton als Basis
            var baseColor = new Color(0.15f, 0.12f, 0.09f);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = baseColor;

            // Diagonale Streifen als Textur-Imitation
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.005f, y * 0.005f);
                    pixels[y * width + x] = Color.Lerp(baseColor,
                        new Color(0.22f, 0.18f, 0.13f), noise * 0.5f);
                }
            }

            // Rand
            int b = 20;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (x < b || x > width - b || y < b || y > height - b)
                        pixels[y * width + x] = new Color(0.08f, 0.06f, 0.04f);

            // Label in der Mitte
            DrawCenteredText(pixels, width, height, sceneId.ToUpper());

            tex.SetPixels(pixels);
            tex.Apply();
            return SpriteManager.Texture2DToSprite(tex);
        }

        // ── Münz-Icon ─────────────────────────────────────────────────────────

        public static Sprite GenerateCoinIcon(int size = 64)
        {
            var tex    = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            int c = size / 2;
            int r = size / 2 - 2;

            var gold   = new Color(1f, 0.8f, 0.1f);
            var dark   = new Color(0.7f, 0.5f, 0f);
            var shine  = new Color(1f, 1f, 0.8f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x-c)*(x-c) + (y-c)*(y-c));
                    if (dist > r) pixels[y*size+x] = Color.clear;
                    else if (dist > r - 3) pixels[y*size+x] = dark;
                    else if (dist < r * 0.4f && y > c && x < c) pixels[y*size+x] = shine;
                    else pixels[y*size+x] = gold;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return SpriteManager.Texture2DToSprite(tex);
        }

        // ── Hinweis-Icon (Glühbirne) ──────────────────────────────────────────

        public static Sprite GenerateHintIcon(int size = 64)
        {
            var tex    = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            int c = size / 2;

            var yellow = new Color(1f, 0.9f, 0.2f);
            var orange = new Color(1f, 0.6f, 0.1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx   = x - c;
                    float dy   = y - c + size * 0.1f;
                    float dist = Mathf.Sqrt(dx*dx + dy*dy);
                    pixels[y*size+x] = dist < size * 0.35f
                        ? Color.Lerp(yellow, orange, dist / (size * 0.35f))
                        : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return SpriteManager.Texture2DToSprite(tex);
        }

        // ── Tier-Farben ───────────────────────────────────────────────────────

        static Color TierColor(int tier) => tier switch
        {
            1 => new Color(0.55f, 0.40f, 0.25f),   // warmes Braun
            2 => new Color(0.30f, 0.50f, 0.70f),   // Blaugrau
            3 => new Color(0.50f, 0.25f, 0.60f),   // Lila
            4 => new Color(0.70f, 0.45f, 0.10f),   // Orange-Bronze
            5 => new Color(0.20f, 0.60f, 0.30f),   // Smaragdgrün
            _ => new Color(0.80f, 0.15f, 0.15f),   // Rot (Max+)
        };

        // ── Pixel-Zeichenroutinen ─────────────────────────────────────────────

        static void DrawIcon(Color[] pixels, string itemId, int center, Color col)
        {
            // Wähle Symbol basierend auf itemId-Schlüsselwörtern
            if      (itemId.Contains("letter") || itemId.Contains("fragment") || itemId.Contains("doc"))
                DrawEnvelope(pixels, center, col);
            else if (itemId.Contains("pipe")   || itemId.Contains("tobacco"))
                DrawPipe(pixels, center, col);
            else if (itemId.Contains("foot")   || itemId.Contains("cast"))
                DrawFootprint(pixels, center, col);
            else if (itemId.Contains("kit")    || itemId.Contains("forensic"))
                DrawStar(pixels, center, col);
            else if (itemId.Contains("glass")  || itemId.Contains("lens"))
                DrawMagnifier(pixels, center, col);
            else if (itemId.Contains("note")   || itemId.Contains("book"))
                DrawBook(pixels, center, col);
            else
                DrawDiamond(pixels, center, col);
        }

        // Brief-Symbol
        static void DrawEnvelope(Color[] p, int c, Color col)
        {
            int w = 36, h = 24;
            for (int y = c-h/2; y < c+h/2; y++)
                for (int x = c-w/2; x < c+w/2; x++)
                {
                    if (y < 0 || y >= Size || x < 0 || x >= Size) continue;
                    float edge = Mathf.Abs(x-c) / (w/2f) + Mathf.Abs(y-c) / (h/2f);
                    p[y*Size+x] = edge < 1.8f ? col : p[y*Size+x];
                }
        }

        // Sherlock-Pfeife
        static void DrawPipe(Color[] p, int c, Color col)
        {
            // Pfeifenkopf
            DrawFilledCircle(p, c-10, c+5, 10, col);
            // Stiel
            for (int x = c-5; x < c+22; x++)
                for (int y = c-2; y < c+3; y++)
                    if (y >= 0 && y < Size && x >= 0 && x < Size)
                        p[y*Size+x] = col;
        }

        // Fußabdruck
        static void DrawFootprint(Color[] p, int c, Color col)
        {
            DrawFilledCircle(p, c, c+8, 14, col);
            for (int i = 0; i < 4; i++)
                DrawFilledCircle(p, c - 12 + i*8, c-14, 5, col);
        }

        // Stern (Forensik-Kit / Max-Tier)
        static void DrawStar(Color[] p, int c, Color col)
        {
            for (int i = 0; i < 5; i++)
            {
                float angle = i * 72f * Mathf.Deg2Rad - Mathf.PI/2;
                int x1 = c + Mathf.RoundToInt(Mathf.Cos(angle) * 22);
                int y1 = c + Mathf.RoundToInt(Mathf.Sin(angle) * 22);
                DrawLine(p, c, c, x1, y1, col, 3);
            }
        }

        // Lupe
        static void DrawMagnifier(Color[] p, int c, Color col)
        {
            DrawCircleOutline(p, c-4, c+4, 14, col, 3);
            DrawLine(p, c+7, c-3, c+20, c-16, col, 3);
        }

        // Buch
        static void DrawBook(Color[] p, int c, Color col)
        {
            for (int y = c-15; y < c+15; y++)
                for (int x = c-12; x < c+12; x++)
                    if (y >= 0 && y < Size && x >= 0 && x < Size)
                        p[y*Size+x] = col;
            // Seiten-Linien
            for (int line = 0; line < 3; line++)
            {
                int ly = c - 8 + line * 7;
                for (int x = c-8; x < c+8; x++)
                    if (ly >= 0 && ly < Size && x >= 0 && x < Size)
                        p[ly*Size+x] = new Color(0,0,0,0.4f);
            }
        }

        // Diamant
        static void DrawDiamond(Color[] p, int c, Color col)
        {
            for (int y = c-18; y < c+18; y++)
                for (int x = c-18; x < c+18; x++)
                    if (Mathf.Abs(x-c) + Mathf.Abs(y-c) < 18)
                        if (y >= 0 && y < Size && x >= 0 && x < Size)
                            p[y*Size+x] = col;
        }

        // Primitive
        static void DrawFilledCircle(Color[] p, int cx, int cy, int r, Color col)
        {
            for (int y = cy-r; y <= cy+r; y++)
                for (int x = cx-r; x <= cx+r; x++)
                    if (y >= 0 && y < Size && x >= 0 && x < Size)
                        if ((x-cx)*(x-cx)+(y-cy)*(y-cy) <= r*r)
                            p[y*Size+x] = col;
        }

        static void DrawCircleOutline(Color[] p, int cx, int cy, int r, Color col, int thickness)
        {
            for (int t = 0; t < thickness; t++)
                for (int y = cy-r-t; y <= cy+r+t; y++)
                    for (int x = cx-r-t; x <= cx+r+t; x++)
                    {
                        if (y < 0 || y >= Size || x < 0 || x >= Size) continue;
                        float d = Mathf.Sqrt((x-cx)*(x-cx)+(y-cy)*(y-cy));
                        if (Mathf.Abs(d - r) <= thickness) p[y*Size+x] = col;
                    }
        }

        static void DrawLine(Color[] p, int x0, int y0, int x1, int y1, Color col, int thickness)
        {
            int steps = Mathf.Max(Mathf.Abs(x1-x0), Mathf.Abs(y1-y0));
            for (int s = 0; s <= steps; s++)
            {
                int x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, s/(float)steps));
                int y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, s/(float)steps));
                DrawFilledCircle(p, x, y, thickness/2, col);
            }
        }

        static void DrawGloss(Color[] p, int c)
        {
            var gloss = new Color(1f, 1f, 1f, 0.25f);
            for (int y = c+4; y < c+18; y++)
                for (int x = c-16; x < c-4; x++)
                {
                    float d = Mathf.Sqrt((x-c)*(x-c)+(y-c)*(y-c));
                    if (d < Size/2-10 && y >= 0 && y < Size && x >= 0 && x < Size)
                        p[y*Size+x] = Color.Lerp(p[y*Size+x], gloss, 0.5f);
                }
        }

        static void DrawCenteredText(Color[] p, int w, int h, string text)
        {
            // Einfache Pixelschrift — nur als visueller Hinweis
            var col = new Color(0.5f, 0.4f, 0.3f, 0.4f);
            int cx  = w / 2 - text.Length * 4;
            int cy  = h / 2;
            for (int i = 0; i < text.Length; i++)
                DrawFilledSquare(p, w, cx + i*8, cy, 3, col);
        }

        static void DrawFilledSquare(Color[] p, int w, int cx, int cy, int r, Color col)
        {
            for (int y = cy-r; y <= cy+r; y++)
                for (int x = cx-r; x <= cx+r; x++)
                    if (y >= 0 && y < Size && x >= 0 && x < Size)
                        p[y*Size+x] = col;
        }
    }
}
