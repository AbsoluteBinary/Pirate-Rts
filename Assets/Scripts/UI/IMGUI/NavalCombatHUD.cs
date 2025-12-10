using UnityEngine;

namespace UI.IMGUI
{
    public class NavalCombatHUD : MonoBehaviour
    {
        public float playerHealth = 100f;
        public int cannonAmmo = 48;
        public float shipSpeed = 0f;
        public float windDirection = 45f;

        [Header("Shooting Feedback")]
        public float flashDuration = 0.2f;
        private float flashTimer;
        private float lastDamage;

        private GUIStyle damageStyle;

        private void Awake()
        {
            damageStyle = new GUIStyle
            {
                fontSize = 48,
                alignment = TextAnchor.MiddleCenter,
                richText = true,
                normal = { textColor = Color.white }   // fallback
            };
        }

        private void Update()
        {
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                Camera.main.transform.localPosition += Random.insideUnitSphere * 0.15f;
            }
        }

        private void OnGUI()
        {
            // --- TOP-LEFT STATUS BOX ---
            GUILayout.BeginArea(new Rect(20, 20, 300, 220), GUI.skin.box);
            GUILayout.Label("<size=22><b>Ship Status</b></size>");
            GUILayout.Label($"Health: {playerHealth:F0} / 100");
            GUILayout.Label($"Ammo: {cannonAmmo}");
            GUILayout.Label($"Speed: {shipSpeed:F1} knots");

            if (GUILayout.Button("FIRE BROADSIDE", GUILayout.Height(40)))
            {
                cannonAmmo -= 24;
            }
            GUILayout.EndArea();

            // --- COMPASS (TOP-RIGHT) ---
            GUILayout.BeginArea(new Rect(Screen.width - 220, 20, 200, 200));
            DrawCompass();
            GUILayout.EndArea();

            // --- DAMAGE POPUP (CENTER) ---
            if (lastDamage > 0f)
            {
                GUI.Label(
                    new Rect(Screen.width * 0.5f - 150, Screen.height * 0.5f - 80, 300, 160),
                    $"<color=red><size=60>-{lastDamage:F0}</size></color>",
                    damageStyle
                );

                lastDamage = Mathf.MoveTowards(lastDamage, 0f, Time.deltaTime * 100f);
            }

            // --- SCREEN SHAKE ---
            if (flashTimer > 0f)
            {
                GUI.matrix = Matrix4x4.TRS(
                    new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0),
                    Quaternion.identity, Vector3.one);
            }

            // Reset matrix so next frame is clean
            GUI.matrix = Matrix4x4.identity;
        }

        private void DrawCompass()
        {
            GUILayout.Label("<b>Compass</b>");

            Texture2D bg = MakeTex(180, 180, new Color(0f, 0f, 0f, 0.7f));
            GUI.DrawTexture(GUILayoutUtility.GetRect(180, 180), bg);

            Vector2 center = new Vector2(90, 90);
            float rad = windDirection * Mathf.Deg2Rad;
            Vector2 wind = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * 60f;

            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(center.x - 5, center.y - 5, 10, 10), MakeTex(10, 10, Color.white));
            GUI.color = Color.cyan;
            GUI.DrawTexture(new Rect(center.x + wind.x - 5, center.y + wind.y - 5, 10, 10), MakeTex(10, 10, Color.cyan));
            GUI.color = Color.white;
        }

        private Texture2D MakeTex(int w, int h, Color col)
        {
            Color[] pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            Texture2D tex = new Texture2D(w, h);
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }

        // Call from Health.TakeDamage()
        public void OnDamageTaken(float amount)
        {
            lastDamage = amount;
            flashTimer = flashDuration;
        }
    }
}
