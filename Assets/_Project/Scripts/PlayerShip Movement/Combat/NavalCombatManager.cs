using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.PlayerShip_Movement.Combat
{
    public class NavalCombatManager : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private Transform player;
        [SerializeField] private float playerHealth = 100f;
        private float currentHealth;
        [SerializeField] GameObject shipGameObject;

        [Header("Turrets")]
        [SerializeField] private Transform[] turretBarrels;
        [SerializeField] private float turretRange = 20f;
        [SerializeField] private float damage = 25f;
        [SerializeField] private float fireRate = 1.5f;
        [SerializeField] private GameObject trailPrefab;

        [Header("UI")]
        [SerializeField] private float flashDuration = 0.5f;
        [SerializeField] private float damageFadeSpeed = 2f;
        private readonly List<DamagePopup> _damagePopups;
        private readonly GUIStyle damageStyle = new GUIStyle();
        private float[] nextFireTimes;

        public NavalCombatManager(List<DamagePopup> damagePopups)
        {
            _damagePopups = damagePopups;
        }

        [System.Serializable]
        public class DamagePopup
        {
            public float amount;
            public float timer;
        }

        private void Awake()
        {
            currentHealth = playerHealth;
            damageStyle.fontSize = 48;
            damageStyle.alignment = TextAnchor.MiddleCenter;
            damageStyle.richText = true;

            if (player == null) player = GameObject.FindWithTag("Player")?.transform;
            nextFireTimes = new float[turretBarrels.Length];
        }

        private void Update()
        {
            for (int i = 0; i < turretBarrels.Length; i++)
            {
                Transform barrel = turretBarrels[i];
                if (barrel == null) continue;

                float sqrDist = (player.position - barrel.position).sqrMagnitude;
                bool inRange = sqrDist <= turretRange * turretRange;

                if (inRange)
                {
                    // Aim
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
                        Fire(barrel);
                        nextFireTimes[i] = Time.time + fireRate;
                    }
                }
            }

            // Fade popups
            for (int i = _damagePopups.Count - 1; i >= 0; i--)
            {
                _damagePopups[i].timer -= Time.unscaledDeltaTime;
                if (_damagePopups[i].timer <= 0f)
                    _damagePopups.RemoveAt(i);
            }
        }

        private void Fire(Transform barrel)
        {
            currentHealth -= damage;
           // OnDamageTaken(damage);

            if (currentHealth <= 0f)
            {
                OnShipDestroyed();
            }

            if (trailPrefab)
            {
                var trail = Instantiate(trailPrefab, barrel.position, Quaternion.identity);
                var lr = trail.GetComponentInChildren<LineRenderer>();
                if (lr)
                {
                    lr.SetPosition(0, barrel.position);
                    lr.SetPosition(1, player.position);
                }
                Destroy(trail, 0.5f);
            }
        }

        private void OnShipDestroyed()
        {
            if (shipGameObject != null)
            {
                shipGameObject.SetActive(false);  // ← Disables only the player ship
                //Debug.Log("Player ship disabled – UI remains active");
            }
            // Later: Add explosions, sinking, game over, etc.
        }

        private void OnGUI()
        {
            // Status box
            GUILayout.BeginArea(new Rect(20, 20, 300, 200), GUI.skin.box);
            GUILayout.Label("<size=22><b>Ship Status</b></size>");
            GUILayout.Label($"Health: {currentHealth:F0} / 100");
            GUILayout.EndArea();

            // Compass with player + turrets (fixed east/west)
            GUILayout.BeginArea(new Rect(Screen.width - 220, 20, 200, 200));
            DrawCompass();
            GUILayout.EndArea();

            // Minimap with player + turrets (fixed east/west)
            GUILayout.BeginArea(new Rect(Screen.width - 220, Screen.height - 220, 200, 200));
            DrawMinimap();
            GUILayout.EndArea();

            // Damage popups
            for (int i = 0; i < _damagePopups.Count; i++)
            {
                float offsetX = (_damagePopups.Count - 1 - i) * 70f;
                Rect r = new Rect(Screen.width * 0.5f - 150 + offsetX, Screen.height * 0.5f - 80, 300, 160);
                GUI.Label(r, $"<color=red><size=60>-{_damagePopups[i].amount:F0}</size></color>", damageStyle);
            }
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

                // FIXED: East/West correct (negate x)
                Vector2 screenDir = new Vector2(-dir.x, dir.z).normalized * 60f;
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
                Vector2 rel = new Vector2(turret.position.x - player.position.x, turret.position.z - player.position.z);
                rel *= 2f;
                // FIXED: East/West correct (negate x)
                Vector2 pos = center + new Vector2(-rel.x, rel.y);
                GUI.DrawTexture(new Rect(pos.x - 4, pos.y - 4, 8, 8), MakeTex(8, 8, Color.red));
            }
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
