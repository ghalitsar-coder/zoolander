using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))] // Memastikan NPC selalu punya komponen NavMeshAgent otomatis
public class NPCController : MonoBehaviour
{
    [Header("Target & Path")]
    public Transform player; 
    public Transform[] waypoints; 

    [Header("Settings")]
    public float chaseDistance = 10f;    
    public float stopChaseDistance = 15f; 

    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Pastikan stopChaseDistance minimal sama dengan chaseDistance (biar ga error logic)
        if (stopChaseDistance < chaseDistance)
        {
            stopChaseDistance = chaseDistance + 2f;
        }

        // Cari player otomatis
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // Jalan ke waypoint awal
        if (waypoints.Length > 0 && waypoints[0] != null && agent.isOnNavMesh)
        {
            agent.SetDestination(waypoints[0].position);
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (isChasing)
        {
            // Jika berhasil kabur
            if (distanceToPlayer > stopChaseDistance)
            {
                ReturnToPatrol();
            }
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            // Jika ketahuan
            if (distanceToPlayer <= chaseDistance)
            {
                StartChasing();
            }
            else
            {
                Patrol();
            }
        }
    }

    void StartChasing()
    {
        isChasing = true;
        ChasePlayer();
    }

    void ChasePlayer()
    {
        if (!agent.isOnNavMesh) return;

        // OPTIMASI: Update jalan ke player HANYA JIKA rute sebelumnya sudah selesai dikalkulasi
        // agar tidak jitter/ngelag dipanggil tiap update frame
        if (!agent.pathPending && Vector3.Distance(agent.destination, player.position) > 1.0f)
        {
            agent.SetDestination(player.position);
        }
    }

    void ReturnToPatrol()
    {
        isChasing = false;

        if (!agent.isOnNavMesh) return;

        if (waypoints.Length > 0 && waypoints[currentWaypointIndex] != null)
        {
            // Lanjut ke waypoint terakhir
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
        else
        {
            // BUG FIX: Jika waypoints ternyata kosong, hentikan agent di posisinya,
            // Jika tidak direset agent bakal terus lari ke lokasi 'ghost' sisaan target player.
            agent.ResetPath(); 
        }
    }

    void Patrol()
    {
        if (waypoints.Length == 0 || !agent.isOnNavMesh) return;

        // Normal patrol: Jika sudah sampe waypoint, ganti target destinasinya
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;

            if (waypoints[currentWaypointIndex] != null)
            {
                agent.SetDestination(waypoints[currentWaypointIndex].position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopChaseDistance);
    }
}