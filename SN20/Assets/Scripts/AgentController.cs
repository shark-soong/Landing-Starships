using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators; // ✅ 추가

[RequireComponent(typeof(RocketController))]
public class AgentController : Agent
{
    public EnvironmentParameters m_ResetParams;
    private RocketController rc;

    public GameObject mainCam;
    public GameObject sideCam;

    void Start()
    {
        rc = GetComponent<RocketController>();
    }

    public override void OnEpisodeBegin()
    {
        m_ResetParams = Academy.Instance.EnvironmentParameters;

        if (m_ResetParams.GetWithDefault("init_height", 50) != rc.height)
            rc.height = m_ResetParams.GetWithDefault("init_height", 50);

        if (m_ResetParams.GetWithDefault("init_angle_roll", 0) != rc.init_angle_roll)
            rc.init_angle_roll = m_ResetParams.GetWithDefault("init_angle_roll", 0);

        if (m_ResetParams.GetWithDefault("init_angle_pitch", 0) != rc.init_angle_pitch)
            rc.init_angle_pitch = m_ResetParams.GetWithDefault("init_angle_pitch", 0);

        if (m_ResetParams.GetWithDefault("init_xoffset", 4) != rc.init_xoffset)
            rc.init_xoffset = m_ResetParams.GetWithDefault("init_xoffset", 4);

        if (m_ResetParams.GetWithDefault("init_zoffset", 4) != rc.init_zoffset)
            rc.init_zoffset = m_ResetParams.GetWithDefault("init_zoffset", 4);

        if (m_ResetParams.GetWithDefault("init_zspeed", 0) != rc.init_zspeed)
            rc.init_zspeed = m_ResetParams.GetWithDefault("init_zspeed", 0);

        if (m_ResetParams.GetWithDefault("angle_tvc", 5) != rc.angle_tvc)
            rc.angle_tvc = m_ResetParams.GetWithDefault("angle_tvc", 5);

        if (m_ResetParams.GetWithDefault("thrust_engine", 4000) != rc.thrust_engine)
            rc.thrust_engine = m_ResetParams.GetWithDefault("thrust_engine", 4000);

        if (m_ResetParams.GetWithDefault("thrust_rcs", 2.5f) != rc.thrust_rcs)
            rc.thrust_rcs = m_ResetParams.GetWithDefault("thrust_rcs", 2.5f);

        if (m_ResetParams.GetWithDefault("collision_speed", 5) != rc.collision_speed)
            rc.collision_speed = m_ResetParams.GetWithDefault("collision_speed", 5);

        rc.ResetPosition();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 rocketPosition = rc.GetPosition();
        Vector3 rocketVelocity = rc.GetVelocity();
        Vector3 rocketAngularVelocity = rc.GetAngularVelocity();
        float rollIndicator = rc.GetZRotation();
        float pitchIndicator = rc.GetXRotation();
        float upsideDownIndicator = rc.GetYRotation();

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

    // ✅ float[] → ActionBuffers 로 변경
    public override void OnActionReceived(ActionBuffers actions)
    {
        rc.ModifyEngineState(actions.DiscreteActions[0], actions.DiscreteActions[1], actions.DiscreteActions[2]);
        rc.ModifyRollRCSState(actions.DiscreteActions[3]);
        rc.ModifyPitchRCSState(actions.DiscreteActions[4]);
    }

    // ✅ RocketController에서 호출하는 EndEpisode 래퍼
    public void EndEpisodeWithReward(float reward)
    {
        AddReward(reward);
        EndEpisode();
    }

    // ✅ float[] → ActionBuffers 로 변경
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        discreteActions[0] = 0;
        if (Input.GetKey(KeyCode.Space)) discreteActions[0] = 1;

        discreteActions[1] = 0;
        discreteActions[2] = 0;
        discreteActions[3] = 0;
        discreteActions[4] = 0;

        if (mainCam != null && mainCam.activeSelf)
        {
            if (Input.GetKey(KeyCode.RightArrow))       discreteActions[3] = 1;
            else if (Input.GetKey(KeyCode.LeftArrow))   discreteActions[3] = 2;

            if (Input.GetKey(KeyCode.Q))                discreteActions[1] = 1;
            else if (Input.GetKey(KeyCode.D))           discreteActions[1] = 2;
        }
        else
        {
            if (Input.GetKey(KeyCode.RightArrow))       discreteActions[4] = 2;
            else if (Input.GetKey(KeyCode.LeftArrow))   discreteActions[4] = 1;

            if (Input.GetKey(KeyCode.Q))                discreteActions[2] = 1;
            else if (Input.GetKey(KeyCode.D))           discreteActions[2] = 2;
        }
    }
}