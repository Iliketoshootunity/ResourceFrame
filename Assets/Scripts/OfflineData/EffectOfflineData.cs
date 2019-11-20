using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectOfflineData : OfflineData {

    public ParticleSystem[] AllParticle;
    public TrailRenderer[] AllTrail;

    public override void ResetProp()
    {
        base.ResetProp();
        foreach (ParticleSystem particle in AllParticle)
        {
            particle.Clear(true);
            particle.Play();
        }

        foreach (TrailRenderer trail in AllTrail)
        {
            trail.Clear();
        }
    }

    public override void BindData()
    {
        base.BindData();
        AllParticle = GetComponentsInChildren<ParticleSystem>(true);
        AllTrail = GetComponentsInChildren<TrailRenderer>(true);

    }
}
