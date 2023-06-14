using AnimationEngine.Language;
using AnimationEngine.Utility;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace AnimationEngine.Core
{
    internal class Emitter : ScriptLib, Initializable, Parentable
    {
        private string dummyName;
        private IMyEntity ent;
        private IMyModelDummy dum;
        private MyEntity3DSoundEmitter soundEmitter;
        private List<MyParticleEffect> effects;
        private string parentSubpart;

        public string GetParent()
        {
            return parentSubpart;
        }

        public Emitter(string dummyName, string parentSubpart)
        {
            this.parentSubpart = parentSubpart;
            effects = new List<MyParticleEffect>();
            this.dummyName = dummyName;
            if (!MyAPIGateway.Utilities.IsDedicated)
            {
                AddMethod("playparticle", PlayParticle);
                AddMethod("stopparticle", StopParticles);
                AddMethod("playsound", PlaySound);
                AddMethod("stopsound", StopSound);
            }
        }

        private void Close(IMyEntity ent)
        {
            StopParticles(null);
            soundEmitter?.Cleanup();
            ent.OnClose -= Close;
        }

        public void Init(IMyEntity ent)
        {
            this.ent = ent;
            ent.OnClose += Close;
            if (!MyAPIGateway.Utilities.IsDedicated)
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
            return false;
        }

        private SVariable StopSound(SVariable[] arr)
        {
            soundEmitter?.StopSound(true);
            return null;
        }

        private SVariable PlaySound(SVariable[] arr)
        {
            if (soundEmitter == null)
            {
                soundEmitter = new MyEntity3DSoundEmitter((MyEntity)ent);
                soundEmitter.Force2D = true;
            }
            soundEmitter.PlaySoundWithDistance(MySoundPair.GetCueId(arr[0].ToString()), true);
            return null;
        }

        private SVariable PlayParticle(SVariable[] arr)
        {
            var p = Create(arr[0].ToString());
            if (p == null)
            {
                return null;
            }

            p.UserScale = arr[1].AsFloat();
            p.UserLifeMultiplier = arr[2].AsFloat();
            p.Autodelete = true;

            if (arr.Length >= 4)
            {
                p.Velocity = arr[3].AsVector3();
            }

            if (arr.Length >= 7)
            {
                p.UserColorMultiplier = new Vector4(arr[4].AsInt(), arr[5].AsInt(), arr[6].AsInt(), 1);
            }

            p.Autodelete = true;
            effects.Add(p);
            p.OnDelete += OnParticleDelete;

            p.Play();
            return null;
        }

        private void OnParticleDelete(MyParticleEffect e)
        {
            effects.Remove(e);
            e.StopLights();
        }

        private SVariable StopParticles(SVariable[] arr)
        {
            foreach (var x in effects)
            {
                x?.Stop();
            }
            return null;
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
            Utils.LogToFile($"Cannot find particle with name '{particle}'");
            return null;
        }
    }
}
