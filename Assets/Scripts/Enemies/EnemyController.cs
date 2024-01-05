using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static UnityEngine.RuleTile.TilingRuleOutput;

public enum AttackType { Melee, Range};
public enum DamageType { Descrete, Continuous};

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    private float damage;

    [SerializeField]
    private DamageType damageType;
    [Header("For continuous damage")]
    [SerializeField]
    private float damageDuration;
    [Space(5)]
    [SerializeField]
    private float attackCooldown;
    [SerializeField]
    private AttackType attackType;
    [Header("For range attack")]
    [SerializeField]
    private GameObject projectilePrefab;

    [Space(5)]
    [SerializeField]
    private bool isMovingRight = true;
    [SerializeField]
    private float moveRange = 0.0f;

    [SerializeField]
    private int points = 6;

    [Space(5)]
    [SerializeField]
    private Slider healthSlider;
    [SerializeField]
    private Canvas healthCanvas;

    private Moving moving;
    private StatsContainer stats;
    private Animator animator;
    private EnemyPathfinding pathfinding;
    private Timer cooldownTimer;

    private GameObject player;
    private StatsContainer playerStats;

    private Vector3 scale;
    private Vector3 healthSliderScale;

    private int direction;
    private float startPositionX;

    private bool triggered = false;
    private bool dead = false;

    private void Awake()
    {

    }

    private void Start()
    {
        moving = GetComponent<Moving>();
        stats = GetComponent<StatsContainer>();
        animator = GetComponent<Animator>();
        pathfinding = GetComponent<EnemyPathfinding>();
        cooldownTimer = gameObject.AddComponent<Timer>();

        player = pathfinding.Player;
        playerStats = player.GetComponent<StatsContainer>();

        cooldownTimer.SetTimer(attackCooldown);

        direction = isMovingRight ? 1 : -1;

        scale = transform.localScale;
        healthSliderScale = healthSlider.transform.localScale;

        if(!isMovingRight)
        {
            Rotate();
        }

        stats.OnFirstHit.AddListener(TriggerEnemy);
        stats.OnZeroHealth.AddListener(KillEnemy);

        healthCanvas.enabled = false;
        healthSlider.maxValue = stats.GetMaxHealth();
        healthSlider.value = healthSlider.maxValue;

        startPositionX = transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        if(!triggered) //basic movement
        {
            if (moveRange != 0.0f)
            {
                CheckDirection();
                transform.Translate(moving.Speed * Time.deltaTime * direction, 0.0f, 0.0f, Space.World);
            }
        }
        else
        {
            healthSlider.value = stats.Health;
            CheckDirection();
            transform.Translate(moving.Speed * pathfinding.Direction.x * Time.deltaTime, moving.Speed * pathfinding.Direction.y * Time.deltaTime, 0f, Space.World);
        }

        if(CanAttack())
        {
            Attack();
        }
    }

    public int Points()
    {
        return points;
    }

    private void CheckDirection()
    {
        if(!triggered)
        {
            if (isMovingRight)
            {
                if (direction == 1 && transform.position.x >= startPositionX + moveRange)
                {
                    direction = -1;
                    Rotate();
                }
                else if (direction == -1 && transform.position.x <= startPositionX)
                {
                    direction = 1;
                    Rotate();
                }
            }
            else
            {
                if (direction == 1 && transform.position.x >= startPositionX)
                {
                    direction = -1;
                    Rotate();
                }
                else if (direction == -1 && transform.position.x <= startPositionX - moveRange)
                {
                    direction = 1;
                    Rotate();
                }
            }
        }
        else
        {
            if(direction == 1 && pathfinding.Direction.x < 0f)
            {
                direction = -1;
                Rotate();
            }
            else if(direction == -1 && pathfinding.Direction.x > 0f)
            {
                direction = 1;
                Rotate();
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision) //vision trigger
    {
        if (collision.CompareTag("Player"))
        {
            TriggerEnemy();
        }
    }

    private bool CanAttack()
    {
        return !cooldownTimer.IsActive() && pathfinding.OnAttackDistance && pathfinding.AboveTarget() && !dead;
    }

    public void TriggerEnemy()
    {
        if(!triggered)
        {
            pathfinding.Activate();
            triggered = true;
            healthCanvas.enabled = true;
        }
    }

    private void Attack()
    {
        if(attackType == AttackType.Melee)
        {
            if(damageType == DamageType.Descrete)
            {
                playerStats.ChangeHP(-damage);
            }
            else if(damageType == DamageType.Continuous)
            {
                StatsEffect.AddEffect(player, StatType.HEALTH, -damage, damageDuration);
            }
        }
        cooldownTimer.Activate();
    }

    private void KillEnemy()
    {
        dead = true;
        GetComponent<Collider2D>().enabled = false;
        animator.SetBool("isDead", true);
        healthCanvas.enabled = false;
        moving.Speed = 0.0f;
        StartCoroutine(KillOnAnimationEnd());
    }

    IEnumerator KillOnAnimationEnd()
    {
        yield return new WaitForSeconds(1);
        gameObject.SetActive(false);
        Destroy(gameObject, 5.0f);
    }

    private void Rotate()
    {
        scale.x *= -1;
        transform.localScale = scale;

        healthSliderScale.x *= -1;
        healthSlider.transform.localScale = healthSliderScale;
    }
}