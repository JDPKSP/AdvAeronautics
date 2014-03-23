using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvAeronautics {
    public class ModuleAdvGimbal : ModuleGimbal {
        [KSPField]
        public string
            FinPivotName = "",
            FinTipName = "",
            FinAnchorName = "";

        FinSet fins;

        bool flightStarted = false;
        public override void OnStart(PartModule.StartState state) {
            base.OnStart(state);
            if (state == StartState.Editor) return;
            flightStarted = true;

            fins = new FinSet();

            foreach (Transform gimbal in part.FindModelTransforms(gimbalTransformName))
                foreach (Transform fin in gimbal.parent)
                    if (fin.name == FinPivotName)
                        fins.Add(new VectorFin(fin, fin.FindChild(FinTipName), gimbal.FindChild(FinAnchorName)));
        }

        public override void OnUpdate() {
            base.OnUpdate();

            if (!flightStarted) return;

            base.OnFixedUpdate();
            fins.Update(isEnabled);
        }
    }
}
