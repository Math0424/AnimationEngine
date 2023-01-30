using AnimationEngine.Utility;
using Sandbox.Game.Entities;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace AnimationEngine.Core
{
    internal class Emitter : Actionable, Initializable
    {
        private string dummyName;
        private IMyEntity ent;
        private IMyModelDummy dum;
        private string parent;
        private MyEntity3DSoundEmitter soundEmitter;
        private List<MyParticleEffect> effects;

        public Emitter(string dummyName, string parent)
        {
            effects = new List<MyParticleEffect>();
            this.parent = parent;
            this.dummyName = dummyName;
            Actions.Add("playparticle", PlayParticle);
            Actions.Add("stopparticle", StopParticles);
            Actions.Add("playsound", PlaySound);
            Actions.Add("stopsound", StopSound);
        }

        public string GetParent()
        {
            return parent;
        }

        private void Close(IMyEntity ent)
        {
            StopParticles(null);
            soundEmitter?.Cleanup();
            ent.OnClose -= Close;
        }

        public void Initalize(IMyEntity ent)
        {
            this.ent = ent;
            ent.OnClose += Close;
            FindDummy(ent);
        }

        private bool FindDummy(IMyEntity ent)
        {
            Dictionary<string, IMyModelDummy> dummies = new Dictionary<string, IMyModelDummy>();
            ent.Model.GetDummies(dummies);
            foreach (var dum in dummies)
            {
                if (dum.Value.Name.Equals(dummyName))
                {
                    this.dum = dum.Value;
                    return true;
                }
            }
            //Utils.LogToFile($"Emitter failed to spawn, could not find dummy '{dummyName}'");
            return false;
        }

        private void StopSound(object[] arr)
        {
            soundEmitter?.StopSound(true);
        }

        private void PlaySound(object[] arr)
        {
            if (soundEmitter == null)
            {
                soundEmitter = new MyEntity3DSoundEmitter((MyEntity)ent);
                soundEmitter.Force2D = true;
            }
            soundEmitter.PlaySoundWithDistance(MySoundPair.GetCueId(arr[0].ToString()));
        }

        private void PlayParticle(object[] arr)
        {
            var p = Create(arr[0] as string);
            if (p == null)
            {
                return;
            }

            p.UserScale = (float)arr[1];
            p.UserLifeMultiplier = (float)arr[2];
            p.Autodelete = true;

            if (arr.Length >= 4)
            {
                p.Velocity = (Vector3)arr[3];
            }

            if (arr.Length >= 7)
            {
                p.UserColorMultiplier = new Vector4((int)arr[4], (int)arr[5], (int)arr[6], 1);
            }

            p.Autodelete = true;
            effects.Add(p);
            p.OnDelete += (e) => { effects.Remove(e); };

            p.Play();
        }

        private void StopParticles(object[] arr)
        {
            foreach (var x in effects)
            {
                x?.Stop();
            }
        }

        private MyParticleEffect Create(string particle)
        {
            if (dum == null && !FindDummy(ent))
            {
                return null;
            }

            MatrixD matrix = dum.Matrix;
            var pos = matrix.Translation;
            MyParticleEffect effect;
            if (MyParticlesManager.TryCreateParticleEffect(particle, ref matrix, ref pos, ent.Render.GetRenderObjectID(), out effect))
            {
                return effect;
            }
            return null;
        }

    }
}
