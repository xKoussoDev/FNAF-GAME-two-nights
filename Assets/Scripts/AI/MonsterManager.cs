using UnityEngine;
using System.Collections;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }

    // ─ Inspector
    [Header("Monster References  (drag AI components here)")]
    [SerializeField] FreddyAI freddy;
    [SerializeField] BonnieAI bonnie;
    [SerializeField] ChicaAI chica;
    [SerializeField] FoxyAI foxy;

    [Header("Night 1")]
    [Tooltip("Seconds before the single random monster activates.")]
    [SerializeField] float night1ActivationDelay = 10f;

    [Header("Night 2  (staggered activation delays)")]
    [Tooltip("Seconds from night start each monster activates. Order: Freddy, Bonnie, Chica, Foxy.")]
    [SerializeField] float[] night2Delays = { 0f, 5f, 10f, 15f };

    // ─ Lifecycle

    void Awake() => Instance = this;

    void Start()
    {
        int night = GameManager.Instance?.CurrentNight ?? 1;
        DeactivateAll();

        if (night == 1)
            StartCoroutine(Night1Setup());
        else
            StartCoroutine(Night2Setup());
    }

    // ─ Setup routines

    IEnumerator Night1Setup()
    {
        yield return new WaitForSeconds(night1ActivationDelay);

        MonsterAI[] all = { freddy, bonnie, chica };

        for (int i = all.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            MonsterAI temp = all[i]; all[i] = all[j]; all[j] = temp;
        }

        for (int i = 0; i < 2; i++)
        {
            if (all[i] != null)
            {
                all[i].Activate(1);
                Debug.Log($"[Night 1] Active monster: {all[i].GetType().Name}");
            }
        }
    }

    IEnumerator Night2Setup()
    {
        MonsterAI[] all = { freddy, bonnie, chica, foxy };

        for (int i = 0; i < all.Length; i++)
        {
            MonsterAI m = all[i];
            float     delay = (i < night2Delays.Length) ? night2Delays[i] : i * 5f;
            StartCoroutine(ActivateAfterDelay(m, 2, delay));
        }

        yield return null;
    }

    IEnumerator ActivateAfterDelay(MonsterAI monster, int night, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        monster?.Activate(night);
    }

    // ─ Helpers

    public void DeactivateAll()
    {
        freddy?.Deactivate();
        bonnie?.Deactivate();
        chica?.Deactivate();
        foxy?.Deactivate();
    }
}
