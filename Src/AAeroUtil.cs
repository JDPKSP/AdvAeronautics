using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvAeronautics {

    public class ModuleAAfixAnim : PartModule {
        public override void OnStart(PartModule.StartState state) {
            int i = 0;
            foreach (AnimationState s in part.FindModelAnimators()[0])
                s.layer = i++;
        }
    }

    public static class AAeroUtil {

        public static ConfigNode Data = new ConfigNode("GearData");


        public static void FindWheelColliders(this Transform transform, ref HashSet<WheelCollider> wheelColliders) {
            WheelCollider tmp = transform.GetComponent<WheelCollider>();
            if (tmp != null)
                wheelColliders.Add(tmp);
            foreach (Transform t in transform)
                t.FindWheelColliders(ref wheelColliders);
        }

        public static Transform findInHierarchy(this Transform t, string name) {
            if (t.name.Equals(name))
                return t;
            foreach (Transform t2 in t) {
                Transform tmp = t2.findInHierarchy(name);
                if (tmp != null)
                    return tmp;
            }

            return null;
        }

        public static WheelCollider findWheelColliderInHierarchy(this Transform t) {

            WheelCollider tmp = t.GetComponent<WheelCollider>();

            if (tmp != null)
                return tmp;
            foreach (Transform t2 in t) {
                tmp = t2.findWheelColliderInHierarchy();
                if (tmp != null)
                    return tmp;
            }

            return null;
        }

        public static void findTransformsWithPrefix(Transform input, ref List<Transform> list, string prefix) {
            if (input.name.ToLower().StartsWith(prefix.ToLower()))
                list.Add(input);
            foreach (Transform t in input)
                findTransformsWithPrefix(t, ref list, prefix);
        }
    }
}
