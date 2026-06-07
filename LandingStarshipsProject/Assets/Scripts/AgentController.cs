using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators; // ← 추가

[RequireComponent(typeof(RocketController))]
public class AgentController : Agent
{
    public EnvironmentParameters m_ResetParams;
    private RocketController rc;

    public bool episodeFinished;

    void Start()
    {
        rc = GetComponent<RocketController>();
    }

    public override void OnEpisodeBegin()
    {
        m_ResetParams = Academy.Instance.EnvironmentParameters;

        rc.height = m_ResetParams.GetWithDefault("init_height", 550);
        rc.init_angle_roll = m_ResetParams.GetWithDefault("init_angle_roll", 5);
        rc.init_angle_pitch = m_ResetParams.GetWithDefault("init_angle_pitch", 90);
        rc.init_xoffset = m_ResetParams.GetWithDefault("init_xoffset", 15);
        rc.init_zoffset = m_ResetParams.GetWithDefault("init_zoffset", 90);
        rc.init_zspeed = m_ResetParams.GetWithDefault("init_zspeed", 1);
        rc.angle_tvc = m_ResetParams.GetWithDefault("angle_tvc", 7);
        rc.thrust_engine = m_ResetParams.GetWithDefault("thrust_engine", 5000);
        rc.thrust_rcs = m_ResetParams.GetWithDefault("thrust_rcs", 0.2f);
        rc.collision_speed = m_ResetParams.GetWithDefault("collision_speed", 5);

        episodeFinished = false;
        rc.ResetPosition();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 rocketPosition = rc.GetPosition();
        Vector3 rocketVelocity = rc.GetVelocity();
        Vector3 rocketAngularVelocity = rc.GetAngularVelocity();
        float rollIndicator = rc.GetRollRotation();
        float pitchIndicator = rc.GetPitchRotation();
        float upsideDownIndicator = rc.GetUpsideDownRotation();

        sensor.AddObservation(rocketPosition.x);
        sensor.AddObservation(rocketPosition.y);
        sensor.AddObservation(rocketPosition.z);

        sensor.AddObservation(rocketVelocity.x);
        sensor.AddObservation(rocketVelocity.y);
        sensor.AddObservation(rocketVelocity.z);

        sensor.AddObservation(rollIndicator);
        sensor.AddObservation(upsideDownIndicator);
        sensor.AddObservation(pitchIndicator);

        sensor.AddObservation(rocketAngularVelocity.x);
        sensor.AddObservation(rocketAngularVelocity.z);
    }

    // ↓ float[] → ActionBuffers 로 변경
    public override void OnActionReceived(ActionBuffers actions)
    {
        var act = actions.DiscreteActions;
        rc.ModifyEngineState(act[0], act[1], act[2]);
        rc.ModifyRollRCSState(act[3]);
        rc.ModifyPitchRCSState(act[4]);
    }

    public void EndEpisode(float reward)
    {
        AddReward(reward);
        episodeFinished = true;
        StartCoroutine(WaitCoroutine());
    }

    IEnumerator WaitCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        EndEpisode();
    }

    // ↓ float[] → in ActionBuffers 로 변경
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var act = actionsOut.DiscreteActions;

        act[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;

        if (Input.GetKey(KeyCode.RightArrow))
            act[3] = 1;
        else if (Input.GetKey(KeyCode.LeftArrow))
            act[3] = 2;
        else
            act[3] = 0;

        if (Input.GetKey(KeyCode.Q))
            act[1] = 1;
        else if (Input.GetKey(KeyCode.D))
            act[1] = 2;
        else
            act[1] = 0;

        if (Input.GetKey(KeyCode.UpArrow))
            act[4] = 2;
        else if (Input.GetKey(KeyCode.DownArrow))
            act[4] = 1;
        else
            act[4] = 0;

        if (Input.GetKey(KeyCode.A))
            act[2] = 1;
        else if (Input.GetKey(KeyCode.E))
            act[2] = 2;
        else
            act[2] = 0;
    }
}