using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvAeronautics {
    [KSPModule("Landing Gear")]
    public class ModuleMultiLandingGear : PartModule, ISteerable {
        [KSPField]
        public Vector2 MaxDragMinMax = Vector2.zero, MinDragMinMax = Vector2.zero;

        [KSPField]
        public string AnimationName = "Deploy";

        [KSPField]
        public bool FixAnimLayers = false;

        protected Animation anim;
        protected AnimationState animState;

        [KSPField]
        public string ID;

        [KSPField]
        private GearHandler gearHandler = new GearHandler();

        private DragManager DragManager;

        [KSPField(guiName = "Gear", isPersistant = true, guiActiveEditor = true, guiActive = true)]
        [UI_Toggle(disabledText = "Retracted", enabledText = "Deployed")]
        public bool Deployed = true;

        [KSPAction("Toggle Gear", KSPActionGroup.Gear)]
        public void GearToggleAction(KSPActionParam param) {
            Deployed = !Deployed;
        }
        [KSPAction("Deploy Gear")]
        public void GearDeployAction(KSPActionParam param) {
            Deployed = true;
        }
        [KSPAction("Retract Gear")]
        public void GearRetractAction(KSPActionParam param) {
            Deployed = false;
        }

        [KSPField(guiName = "Brakes", isPersistant = true, guiActiveEditor = true, guiActive = true)]
        [UI_Toggle(disabledText = "Disengaged", enabledText = "Engaged")]
        public bool Brakes = false;

        [KSPAction("Brakes", KSPActionGroup.Brakes)]
        public void BrakesAction(KSPActionParam param) {
            Brakes = param.type == KSPActionType.Activate;
        }

        private ModuleLandingGear.GearStates gearState = ModuleLandingGear.GearStates.UNDEFINED;

        public bool isSteerable {
            get {
                return gearState == ModuleLandingGear.GearStates.DEPLOYED;
            }
        }

        public void UpdateDrag() {
            DragManager.SetDrag(this, new DragValue(
                (MaxDragMinMax.x * (1 - animState.normalizedTime)) + (MaxDragMinMax.y * animState.normalizedTime),
                (MinDragMinMax.x * (1 - animState.normalizedTime)) + (MinDragMinMax.y * animState.normalizedTime)
                ));
        }

        public override void OnLoad(ConfigNode node) {
            if (AAeroUtil.Data.HasNode(ID)) return;

            ConfigNode tmpnode = new ConfigNode(ID);

            foreach (ConfigNode n in node.GetNodes("Gear"))
                tmpnode.AddNode(n);
            if (tmpnode.GetNodes("Gear").Length > 0)
            AAeroUtil.Data.AddNode(tmpnode);
        }

        public override void OnInitialize() {
            if (!Deployed) {
                Transform bounds = base.part.FindModelTransform("Bounds");
                if (bounds == null) return;
                bounds.collider.enabled = false;
                UnityEngine.Object.DestroyImmediate(bounds.gameObject);
            }
        }

        public override void OnStart(PartModule.StartState state) {

            DragManager = part.gameObject.GetComponent<DragManager>();

            if (DragManager == null) {
                DragManager = part.gameObject.AddComponent<DragManager>();
                DragManager.SetPart(part);
            }

            gearHandler.Start(ID,part);
            anim = part.FindModelAnimators(AnimationName)[0];
            animState = anim[AnimationName];
            animState.wrapMode = WrapMode.Clamp;

            if (FixAnimLayers) {
                int i = 0;
                foreach (AnimationState s in anim)
                    s.layer = i++;
            }

            part.PhysicsSignificance = 1;

            Transform bounds = part.FindModelTransform("Bounds");
            if (bounds != null)
                UnityEngine.Object.DestroyImmediate(bounds.gameObject);

            gearState = Deployed ? ModuleLandingGear.GearStates.DEPLOYED : ModuleLandingGear.GearStates.RETRACTED;

            animState.normalizedTime = Deployed ? 1 : 0;

            animState.speed = Deployed ? Mathf.Abs(animState.speed) : -Mathf.Abs(animState.speed);

            UpdateDrag();

            anim.Play(AnimationName);

            HashSet<WheelCollider> tmp = new HashSet<WheelCollider>();
            part.transform.FindWheelColliders(ref tmp);
            foreach (WheelCollider w in tmp)
                w.enabled = true;

        }

        public void LateUpdate() {
            if (!part.packed && gearState == ModuleLandingGear.GearStates.DEPLOYED)
                gearHandler.SuspensionUpdate();
        }

        private void Update() {
                if (part.packed && HighLogic.LoadedSceneIsFlight) return;

                if (!Deployed && gearState == ModuleLandingGear.GearStates.DEPLOYED) {
                    gearState = ModuleLandingGear.GearStates.RETRACTING;
                    animState.normalizedTime = 1;
                    animState.speed = -Math.Abs(animState.speed);
                    anim.Play(AnimationName);
                }
                else if (!Deployed && gearState == ModuleLandingGear.GearStates.DEPLOYING) {
                    gearState = ModuleLandingGear.GearStates.RETRACTING;
                    animState.speed = -Math.Abs(animState.speed);
                    anim.Play(AnimationName);
                }
                else if (Deployed && gearState == ModuleLandingGear.GearStates.RETRACTED) {
                    gearState = ModuleLandingGear.GearStates.DEPLOYING;
                    animState.normalizedTime = 0;
                    animState.speed = Math.Abs(animState.speed);
                    anim.Play(AnimationName);
                }
                else if (Deployed && gearState == ModuleLandingGear.GearStates.RETRACTING) {
                    gearState = ModuleLandingGear.GearStates.DEPLOYING;
                    animState.speed = Math.Abs(animState.speed);
                    anim.Play(AnimationName);
                }

                if (!anim.IsPlaying(AnimationName)) {
                    if (gearState == ModuleLandingGear.GearStates.DEPLOYING) {
                        gearState = ModuleLandingGear.GearStates.DEPLOYED;
                        DragManager.SetDrag(this, new DragValue(MaxDragMinMax.y, MinDragMinMax.y));
                    }
                    else if (gearState == ModuleLandingGear.GearStates.RETRACTING) {
                        gearState = ModuleLandingGear.GearStates.RETRACTED;
                        gearHandler.ResetSuspension();
                        DragManager.SetDrag(this, new DragValue(MaxDragMinMax.x, MinDragMinMax.x));
                    }
                }
                else
                    UpdateDrag();
                gearHandler.UpdateBrakes(Brakes);            
        }

    }
}
