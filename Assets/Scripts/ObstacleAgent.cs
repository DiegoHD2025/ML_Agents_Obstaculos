using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ObstacleAgent : Agent
{
    public Transform target;
    public float moveSpeed = 5f;

    private Rigidbody rb;

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

        // Colocar el objetivo en su posición inicial
        target.localPosition = new Vector3(0f, 0.5f, 3f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Posición del objetivo respecto al agente
        Vector3 targetPosition = target.localPosition - transform.localPosition;

        sensor.AddObservation(targetPosition);

        // Velocidad del agente
        sensor.AddObservation(rb.velocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Vector3 movement = new Vector3(moveX, 0f, moveZ);

        rb.AddForce(movement * moveSpeed);

	AddReward(-0.001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Target"))
        {
            AddReward(1.0f);

            EndEpisode();
        }
    }
}