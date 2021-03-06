﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvAeronautics {
    public struct DragValue {
        public float maximum_drag;
        public float minimum_drag;

        public DragValue(float maximum_drag, float minimum_drag) {
            this.maximum_drag = maximum_drag;
            this.minimum_drag = minimum_drag;
        }
    }    

    public class DragManager : MonoBehaviour {

        private float maximum_drag, minimum_drag;

        private Dictionary<PartModule, DragValue> ModifyingModules = new Dictionary<PartModule, DragValue>();

        private Part part;

        public void SetPart(Part p){
            part = p;
            maximum_drag = part.maximum_drag;
            minimum_drag = part.minimum_drag;
        }

        public void SetDrag(PartModule pm, DragValue DMaxMin) {
            ModifyingModules[pm] = DMaxMin;

            float Max = maximum_drag;
            float Min = minimum_drag;

            foreach (KeyValuePair<PartModule, DragValue> pair in ModifyingModules) {
                Max += pair.Value.maximum_drag;
                Min += pair.Value.minimum_drag;
            }

            part.maximum_drag = Max < 0 ? 0 : Max;
            part.minimum_drag = Min < 0 ? 0 : Min;
        }
    }
}
