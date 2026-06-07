using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AgentController))]
[RequireComponent(typeof(PhysicDebugger))]
public class RocketController : MonoBehaviour
{
    public ColorizePlane cp;
    public TVCAnimationController tvcac_engine_1;
    public TVCAnimationController tvcac_engine_2;
    public TVCAnimationController tvcac_engine_3;
    public Vector3 centerOfMass;

    private Rigidbody rb;
    private AgentController ac;
    private PhysicDebugger pd;
    private TextController tc;
    private AnimationController anc;
    private EngineParticleController epc;
    private RCSParticleController rpc;
    private LandingSmokeParticleController lspc;
    private EngineRCSAudioController erac;

    bool toReset;
    bool isEngineOn;
    float rollTVC;
    float pitchTVC;
    bool isRollPosRCSOn;
    bool isRollNegRCSOn;
    bool isPitchPosRCSOn;
    bool isPitchNegRCSOn;

    public float height;
    public float init_angle_roll;
    public float init_angle_pitch;
    public float init_xoffset;
    public float init_zoffset;
    public float init_zspeed;
    public float angle_tvc;
    public float thrust_engine;
    public float thrust_rcs;
    public float collision_speed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ac = GetComponent<AgentController>();
        pd = GetComponent<PhysicDebugger>();
        tc = GetComponent<TextController>();
        anc = GetComponent<AnimationController>();
        epc = GetComponent<EngineParticleController>();
        rpc = GetComponent<RCSParticleController>();
        lspc = GetComponent<LandingSmokeParticleController>();
        erac = GetComponent<EngineRCSAudioController>();

        rb.centerOfMass = centerOfMass;
    }

    void FixedUpdate()
    {
        if (ac.episodeFinished) return;

        if (toReset)
        {
            rb.position = transform.parent.transform.TransformPoint(
                new Vector3(
                    Random.Range(-init_xoffset, init_xoffset),
                    height,
                    Random.Range(-init_zoffset, init_zoffset)  // ← 버그 수정: -init_zoffset ~ +init_zoffset
                ));
            rb.velocity = new Vector3(0, 0, init_zspeed);
            rb.rotation = Quaternion.Euler(Random.Range(85, init_angle_pitch), 0, Random.Range(-init_angle_roll, init_angle_roll));
            rb.angularVelocity = Vector3.zero;

            isEngineOn = false;
            rollTVC = 0;
            pitchTVC = 0;
            isRollPosRCSOn = false;
            isRollNegRCSOn = false;
            isPitchPosRCSOn = false;
            isPitchNegRCSOn = false;

            anc.ResetAnimation();
            toReset = false;
            return;
        }

        if (rb.position.y < 1.821 &&
            Mathf.Abs(Vector3.Dot(transform.up, Vector3.right)) < 0.1 &&
            Mathf.Abs(Vector3.Dot(transform.up, Vector3.forward)) < 0.1)
        {
            isRollPosRCSOn = false;
            isRollNegRCSOn = false;
            isPitchPosRCSOn = false;
            isPitchNegRCSOn = false;
        }

        if (isEngineOn)
        {
            Vector3 thrust = new Vector3(0, thrust_engine * Time.deltaTime * 2, 0);
            Quaternion rotation = Quaternion.Euler(pitchTVC, 0, rollTVC);
            Vector3 vectored_thrust = rotation * thrust;
            Vector3 worldForce = transform.TransformVector(vectored_thrust);
            Vector3 worldPoint = transform.TransformPoint(new Vector3(0, -1, 0));
            rb.AddForceAtPosition(worldForce, worldPoint, ForceMode.Force);
            pd.DrawEngineRays(worldForce, worldPoint, thrust_engine / 1000);
        }

        if (isRollPosRCSOn)
        {
            Vector3 thrust_left = new Vector3(thrust_rcs, 0, 0);
            Vector3 wf1 = transform.TransformVector(thrust_left);
            Vector3 wf2 = transform.TransformVector(thrust_left);
            Vector3 wp1 = transform.TransformPoint(new Vector3(0, 41, 4));
            Vector3 wp2 = transform.TransformPoint(new Vector3(0, 41, -4));
            rb.AddForceAtPosition(wf1, wp1, ForceMode.Force);
            rb.AddForceAtPosition(wf2, wp2, ForceMode.Force);
            pd.DrawEngineRays(wf1, wp1, 1);
            pd.DrawEngineRays(wf2, wp2, 1);
        }

        if (isRollNegRCSOn)
        {
            Vector3 thrust_right = new Vector3(-thrust_rcs, 0, 0);
            Vector3 wf1 = transform.TransformVector(thrust_right);
            Vector3 wf2 = transform.TransformVector(thrust_right);
            Vector3 wp1 = transform.TransformPoint(new Vector3(0, 41, 4));
            Vector3 wp2 = transform.TransformPoint(new Vector3(0, 41, -4));
            rb.AddForceAtPosition(wf1, wp1, ForceMode.Force);
            rb.AddForceAtPosition(wf2, wp2, ForceMode.Force);
            pd.DrawEngineRays(wf1, wp1, 1);
            pd.DrawEngineRays(wf2, wp2, 1);
        }

        if (isPitchPosRCSOn)
        {
            Vector3 thrust_fwd = new Vector3(0, 0, thrust_rcs);
            Vector3 wf1 = transform.TransformVector(thrust_fwd);
            Vector3 wf2 = transform.TransformVector(thrust_fwd);
            Vector3 wp1 = transform.TransformPoint(new Vector3(4, 41, 0));
            Vector3 wp2 = transform.TransformPoint(new Vector3(-4, 41, 0));
            rb.AddForceAtPosition(wf1, wp1, ForceMode.Force);
            rb.AddForceAtPosition(wf2, wp2, ForceMode.Force);
            pd.DrawEngineRays(wf1, wp1, 1);
            pd.DrawEngineRays(wf2, wp2, 1);
        }

        if (isPitchNegRCSOn)
        {
            Vector3 thrust_back = new Vector3(0, 0, -thrust_rcs);
            Vector3 wf1 = transform.TransformVector(thrust_back);
            Vector3 wf2 = transform.TransformVector(thrust_back);
            Vector3 wp1 = transform.TransformPoint(new Vector3(4, 41, 0));
            Vector3 wp2 = transform.TransformPoint(new Vector3(-4, 41, 0));
            rb.AddForceAtPosition(wf1, wp1, ForceMode.Force);
            rb.AddForceAtPosition(wf2, wp2, ForceMode.Force);
            pd.DrawEngineRays(wf1, wp1, 1);
            pd.DrawEngineRays(wf2, wp2, 1);
        }

        // 범위 이탈 시 에피소드 종료
        if (rb.position.y > height + 50 ||
            rb.position.y < -1 ||
            Mathf.Abs(transform.parent.transform.InverseTransformPoint(rb.position).x) > 30 ||
            Mathf.Abs(transform.parent.transform.InverseTransformPoint(rb.position).z) > 100)
        {
            cp.Colorize(Color.red);
            ac.EndEpisode(0);
        }

        if (rb.IsSleeping())
        {
            if (Mathf.Abs(Vector3.Dot(transform.up, Vector3.right)) < 0.1 &&
                Mathf.Abs(Vector3.Dot(transform.up, Vector3.forward)) < 0.1 &&
                Vector3.Dot(transform.up, Vector3.up) > 0.9)
            {
                cp.Colorize(Color.green);
                ac.EndEpisode(1);
            }
            else
            {
                cp.Colorize(Color.red);
                ac.EndEpisode(0);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "AgentRocket") return;

        if (collision.relativeVelocity.y > collision_speed)
        {
            cp.Colorize(Color.red);
            ac.EndEpisode(0);
        }
    }

    public void ResetPosition() { toReset = true; }

    public Vector3 GetPosition() => transform.parent.transform.InverseTransformPoint(rb.position);
    public Vector3 GetVelocity() => rb.velocity;
    public Vector3 GetAngularVelocity() => rb.angularVelocity;
    public float GetRollRotation() => Vector3.Dot(transform.up, Vector3.right);
    public float GetUpsideDownRotation() => Vector3.Dot(transform.up, Vector3.up);
    public float GetPitchRotation() => Vector3.Dot(transform.up, Vector3.forward);

    public void ModifyEngineState(int throttleConsigne, int rollTVCConsigne, int pitchTVCConsigne)
    {
        if (throttleConsigne == 0)
        {
            isEngineOn = false;
            rollTVC = 0;
            pitchTVC = 0;
        }
        else
        {
            isEngineOn = true;
            rollTVC = rollTVCConsigne == 1 ? -angle_tvc : rollTVCConsigne == 2 ? angle_tvc : 0;
            pitchTVC = pitchTVCConsigne == 1 ? -angle_tvc : pitchTVCConsigne == 2 ? angle_tvc : 0;
        }
    }

    public void ModifyRollRCSState(int consigne)
    {
        isRollPosRCSOn = consigne == 1;
        isRollNegRCSOn = consigne == 2;
    }

    public void ModifyPitchRCSState(int consigne)
    {
        isPitchPosRCSOn = consigne == 1;
        isPitchNegRCSOn = consigne == 2;
    }
}