using System.Collections.Generic;
using UnityEngine;

namespace PlayerShip_Movement.Combat
{
    public class NavalCombatManager : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private Transform player;
        [SerializeField] private float playerHealth = 100f;
        private float currentHealth;

        [Header("Turrets")]
        [SerializeField] private Transform[] turretBarrels;  // Drag all turret barrels here
        [SerializeField] private float turretRange = 20f;
        [SerializeField] private float damage = 25f;
        [SerializeField] private float fireRate = 1.5f;
        [SerializeField] private GameObject trailPrefab;

        [Header("UI")]
        [SerializeField] private float flashDuration = 0.5f;
        [SerializeField] private float damageFadeSpeed = 2f;
        private List<DamagePopup> damagePopups = new List<DamagePopup>();
        private float flashTimer;
        private float lastDamage;

        private GUIStyle damageStyle;
        private float[] nextFireTimes;

        [System.Serializable]  // ← NEW: Inspector-visible if needed
        private class DamagePopup
        {
            public float amount;
            public float timer;
        }
        
        private void Awake()
        {
            currentHealth = playerHealth;
            damageStyle = new GUIStyle
            {
                fontSize = 48,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };

            if (player == null) player = GameObject.FindWithTag("Player")?.transform;
            nextFireTimes = new float[turretBarrels.Length];
        }

        private void Update()
        {
            // Screen shake
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                Camera.main.transform.localPosition += Random.insideUnitSphere * 0.15f;
            }

            // Turret aiming + firing
            for (int i = 0; i < turretBarrels.Length; i++)
            {
                Transform barrel = turretBarrels[i];
                if (barrel == null) continue;

                float sqrDist = (player.position - barrel.position).sqrMagnitude;
                bool inRange = sqrDist <= turretRange * turretRange;

                // Aim only when in range
                if (inRange)
                {
                    Vector3 toPlayer = player.position - barrel.position;
                    toPlayer.y = 0;
                    if (toPlayer.sqrMagnitude > 0.001f)
                    {
                        Quaternion rot = Quaternion.LookRotation(toPlayer, Vector3.up);
                        barrel.rotation = Quaternion.Euler(90f, rot.eulerAngles.y, 0f);
                    }

                    // Fire
                    if (Time.time >= nextFireTimes[i])
                    {
                        Fire(barrel, player.position);
                        nextFireTimes[i] = Time.time + fireRate;
                    }
                }
            }
        }

        private void Fire(Transform barrel, Vector3 targetPos)
        {
            Vector3 hitPoint = targetPos;
            currentHealth -= damage;
            OnDamageTaken(damage);

            if (trailPrefab)
            {
                var trail = Instantiate(trailPrefab, barrel.position, Quaternion.identity);
                var lr = trail.GetComponentInChildren<LineRenderer>();
                if (lr)
                {
                    lr.SetPosition(0, barrel.position);
                    lr.SetPosition(1, hitPoint);
                }
                Destroy(trail, 0.5f);
            }
        }

        private void OnDamageTaken(float amount)
        {
            damagePopups.Add(new DamagePopup { amount = amount, timer = flashDuration });
            flashTimer = flashDuration;
        }

        private void OnGUI()
        {
            // Status box
            GUILayout.BeginArea(new Rect(20, 20, 300, 200), GUI.skin.box);
            GUILayout.Label("<size=22><b>Ship Status</b></size>");
            GUILayout.Label($"Health: {currentHealth:F0} / 100");
            GUILayout.EndArea();

            // Compass with player + turrets
            GUILayout.BeginArea(new Rect(Screen.width - 220, 20, 200, 200));
            DrawCompass();
            GUILayout.EndArea();

            // Minimap with player + turrets
            GUILayout.BeginArea(new Rect(Screen.width - 220, Screen.height - 220, 200, 200));
            DrawMinimap();
            GUILayout.EndArea();

            for (int i = damagePopups.Count - 1; i >= 0; i--)  // Reverse to avoid index shift
            {
                DamagePopup popup = damagePopups[i];
                popup.timer -= Time.unscaledDeltaTime;  // Unscaled for consistent fade

                if (popup.timer > 0f)
                {
                    // Position: stagger horizontally for overlap prevention
                    float offsetX = (damagePopups.Count - i - 1) * 60f;  // Spread left/right
                    Rect rect = new Rect(
                        Screen.width * 0.5f - 150 + offsetX,
                        Screen.height * 0.5f - 80,
                        300, 160
                    );

                    GUI.Label(rect, $"<color=red><size=60>-{popup.amount:F0}</size></color>", damageStyle);
                    damagePopups[i] = popup;  // Update list
                }
                else
                {
                    damagePopups.RemoveAt(i);  // Clean up expired
                }
            }

            if (flashTimer > 0f)
            {
                GUI.matrix = Matrix4x4.TRS(
                    new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0),
                    Quaternion.identity, Vector3.one);
            }
            GUI.matrix = Matrix4x4.identity;
        }

        private void DrawCompass()
        {
            GUILayout.Label("<b>Compass</b>");
            Texture2D bg = MakeTex(180, 180, new Color(0f, 0f, 0f, 0.7f));
            GUI.DrawTexture(GUILayoutUtility.GetRect(180, 180), bg);

            Vector2 center = new Vector2(90, 90);
            foreach (var turret in turretBarrels)
            {
                if (turret == null) continue;
                Vector3 dir = turret.position - player.position;
                dir.y = 0;
                if (dir.sqrMagnitude < 0.1f) continue;
                Vector2 screenDir = new Vector2(dir.x, dir.z).normalized * 60f;
                GUI.DrawTexture(new Rect(center.x + screenDir.x - 5, center.y + screenDir.y - 5, 10, 10), MakeTex(10, 10, Color.red));
            }
        }

        private void DrawMinimap()
        {
            GUILayout.Label("<b>Minimap</b>");
            Texture2D bg = MakeTex(180, 180, new Color(0.1f, 0.2f, 0.4f, 0.8f));
            GUI.DrawTexture(GUILayoutUtility.GetRect(180, 180), bg);

            Vector2 center = new Vector2(90, 90);
            GUI.DrawTexture(new Rect(center.x - 5, center.y - 5, 10, 10), MakeTex(10, 10, Color.green));

            foreach (var turret in turretBarrels)
            {
                if (turret == null) continue;
                Vector2 pos = WorldToMinimap(turret.position);
                GUI.DrawTexture(new Rect(pos.x - 4, pos.y - 4, 8, 8), MakeTex(8, 8, Color.red));
            }
        }

        private Vector2 WorldToMinimap(Vector3 worldPos)
        {
            Vector2 relative = new Vector2(worldPos.x - player.position.x, worldPos.z - player.position.z);
            relative *= 2f;
            return new Vector2(90, 90) + relative;
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
    }
}
