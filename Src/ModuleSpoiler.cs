using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvAeronautics {
    public class ModuleSpoiler : PartModule {
        [KSPField]
        public string
            AnimationName = "";

        [KSPField]
        public Vector2 MaxDragMinMax = Vector2.zero, MinDragMinMax = Vector2.zero;

        [KSPField]
        public bool FixAnimLayers = false;

        [KSPField(guiName = "Spoiler %", isPersistant = true, guiActive = true, guiActiveEditor = true)]
        [UI_FloatRange(stepIncrement = 1, maxValue = 100, minValue = 0)]
        public float Drag = 100;

        [KSPField(guiName = "Spolier", isPersistant = true, guiActiveEditor = true, guiActive = true)]
        [UI_Toggle(disabledText = "Disengaged", enabledText = "Engaged")]
        public bool engaged = false;

        [KSPAction("Spoiler", KSPActionGroup.Brakes)]
        public void BrakesAction(KSPActionParam param) {
            engaged = param.type == KSPActionType.Activate && Drag > 0;
        }

        ModuleLandingGear.GearStates spoilerState = ModuleLandingGear.GearStates.UNDEFINED;

        private DragManager DragManager;

        protected Animation anim;
        protected AnimationState animState;

        public override void OnStart(PartModule.StartState state) {

            DragManager = part.gameObject.GetComponent<DragManager>();

            if (DragManager == null) {
                DragManager = part.gameObject.AddComponent<DragManager>();
                DragManager.SetPart(part);
            }

            anim = part.FindModelAnimators(AnimationName)[0];
            animState = anim[AnimationName];
            animState.wrapMode = WrapMode.Clamp;

            if (FixAnimLayers) {
                int i = 0;
                foreach (AnimationState s in anim)
                    s.layer = i++;
            }


            animState.normalizedSpeed = 0;
            if (engaged) {
                animState.normalizedTime = Drag / 100;
                spoilerState = ModuleLandingGear.GearStates.DEPLOYED;
            }
            else {
                animState.normalizedTime = 0;
                spoilerState = ModuleLandingGear.GearStates.RETRACTED;
            }
            anim.Play(AnimationName);

        }

        float lastNormalizedTime = 200;

        private void Update() {
            if (part.packed && HighLogic.LoadedSceneIsFlight) return;

            float animSpeed = (HighLogic.LoadedSceneIsFlight ? TimeWarp.deltaTime * 60 : 1) / animState.clip.frameRate;

            if (engaged && Drag == 0) {
                engaged = false;
                spoilerState = ModuleLandingGear.GearStates.RETRACTING;
            }

            float DragPercentage = Drag / 100;
            switch (spoilerState) {
                case ModuleLandingGear.GearStates.DEPLOYED:
                    if (!engaged)
                        spoilerState = ModuleLandingGear.GearStates.RETRACTING;
                    else {
                        if (animState.normalizedTime != DragPercentage) {
                            animState.normalizedTime = Mathf.Clamp(
                                animState.normalizedTime + ((animState.normalizedTime < DragPercentage ? 1 : -1) * animSpeed)
                                , 0, animState.normalizedTime < DragPercentage ? DragPercentage : 1);
                        }
                    }
                    break;
                case ModuleLandingGear.GearStates.DEPLOYING:
                    if (!engaged)
                        spoilerState = ModuleLandingGear.GearStates.RETRACTING;
                    else {
                        if (animState.normalizedTime == DragPercentage) {
                            spoilerState = ModuleLandingGear.GearStates.DEPLOYED;
                        }
                        else {
                            animState.normalizedTime = Mathf.Clamp(
                                animState.normalizedTime + ((animState.normalizedTime < DragPercentage ? 1 : -1) * animSpeed)
                                , 0, animState.normalizedTime < DragPercentage ? DragPercentage : 1);
                        }
                    }
                    break;
                case ModuleLandingGear.GearStates.RETRACTING:
                    if (engaged)
                        spoilerState = ModuleLandingGear.GearStates.DEPLOYING;
                    else {
                        if (animState.normalizedTime == 0) {
                            spoilerState = ModuleLandingGear.GearStates.RETRACTED;
                        }
                        else {
                            animState.normalizedTime = Mathf.Clamp(
                                animState.normalizedTime - animSpeed
                                , 0, DragPercentage);
                        }
                    }
                    break;
                case ModuleLandingGear.GearStates.RETRACTED:
                    if (engaged)
                        spoilerState = ModuleLandingGear.GearStates.DEPLOYING;
                    break;
            }

            if (animState.normalizedTime != lastNormalizedTime) {
                lastNormalizedTime = animState.normalizedTime;
                DragManager.SetDrag(this, new DragValue(
                    (MaxDragMinMax.x * (1 - animState.normalizedTime)) + (MaxDragMinMax.y * animState.normalizedTime),
                    (MinDragMinMax.x * (1 - animState.normalizedTime)) + (MinDragMinMax.y * animState.normalizedTime)
                ));
            }

        }

    }
}
