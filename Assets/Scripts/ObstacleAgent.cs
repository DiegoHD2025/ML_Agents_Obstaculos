using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ObstacleAgent : Agent
{
    public Transform target;
    public float moveSpeed = 5f;
    public Transform[] obstacles;

    private Rigidbody rb;
    private float previousDistanceToTarget;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Detener el movimiento
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Colocar al agente en su posición inicial
        transform.localPosition = new Vector3(0f, 0.5f, -3f);

        // Colocar el Target aleatoriamente
        float randomX = Random.Range(-4f, 4f);
        float randomZ = Random.Range(1f, 4f);

        target.localPosition = new Vector3(randomX, 0.5f, randomZ);

        // Reposicionar obstáculos
        RepositionObstacles();

        // Guardar distancia inicial al Target
        previousDistanceToTarget = Vector3.Distance(
            transform.localPosition,
            target.localPosition
        );
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Posición del objetivo respecto al agente
        Vector3 targetPosition = target.localPosition - transform.localPosition;

        sensor.AddObservation(targetPosition);

        // Velocidad del agente
        sensor.AddObservation(rb.velocity);
    }

    private void RepositionObstacles()
    {
        for (int i = 0; i < obstacles.Length; i++)
        {
            bool validPosition = false;
            int attempts = 0;

            while (!validPosition && attempts < 50)
            {
                attempts++;

                float randomX = Random.Range(-3.5f, 3.5f);
                float randomZ = Random.Range(-1f, 3.5f);

                Vector3 newPosition = new Vector3(
                    randomX,
                    0.5f,
                    randomZ
                );

                validPosition = true;

                // Evitar la posición inicial del Agent
                Vector3 agentStart = new Vector3(0f, 0.5f, -3f);

                if (Vector3.Distance(newPosition, agentStart) < 1.5f)
                {
                    validPosition = false;
                }

                // Evitar el Target
                if (Vector3.Distance(newPosition, target.localPosition) < 1.5f)
                {
                    validPosition = false;
                }

                // Evitar otros obstáculos
                for (int j = 0; j < i; j++)
                {
                    if (Vector3.Distance(
                        newPosition,
                        obstacles[j].localPosition) < 1.5f)
                    {
                        validPosition = false;
                        break;
                    }
                }

                if (validPosition)
                {
                    obstacles[i].localPosition = newPosition;
                }
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Vector3 movement = new Vector3(moveX, 0f, moveZ);

        rb.AddForce(movement * moveSpeed);

        // Calcular distancia actual al Target
        float currentDistanceToTarget = Vector3.Distance(
            transform.localPosition,
            target.localPosition
        );

        // Recompensar si se acerca
        if (currentDistanceToTarget < previousDistanceToTarget)
        {
            AddReward(0.01f);
        }
        else
        {
            // Penalizar ligeramente si se aleja
            AddReward(-0.01f);
        }

        // Guardar la nueva distancia
        previousDistanceToTarget = currentDistanceToTarget;

        // Pequeña penalización por consumir tiempo
        AddReward(-0.0001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Llegó al Target
        if (collision.gameObject.CompareTag("Target"))
        {
            AddReward(1.0f);
            EndEpisode();
        }

        // Chocó contra un obstáculo
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            AddReward(-0.5f);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Entró en la DeathZone
        if (other.CompareTag("DeathZone"))
        {
            AddReward(-1.0f);
            EndEpisode();
        }
    }
}

