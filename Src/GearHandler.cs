using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvAeronautics {
    public class GearHandler : HashSet<Gear> {

        public void Load(ConfigNode n) {
            foreach (ConfigNode g in n.GetNodes("Gear"))
                Add(g);
        }

        public void Add(ConfigNode n) {
            Add(new Gear(n));
        }

        public void Start(string ID, Part part) {
            foreach (ConfigNode g in AAeroUtil.Data.GetNode(ID).GetNodes("Gear"))
                Add(g);
            foreach (Gear g in this)
                g.Start(part);
        }

        public void UpdateBrakes(bool brakes) {
            foreach (Gear g in this)
                g.UpdateBrakes(brakes);
        }
        public void SuspensionUpdate() {
            foreach (Gear g in this)
                g.SuspensionUpdate();
        }

        public void ResetSuspension() {
            foreach (Gear g in this)
                g.ResetSuspension();
        }


        public void Save(ConfigNode node) {
            throw new NotImplementedException();
        }
    }


    public class Gear {
        public string
            wheelName = "Wheel",
            suspensionParentName = "suspensionParent";
        public float
            BrakeTorque = 15f,
            BrakeSpeed = 0.5f;

        private Transform
            wheelTransform,
            suspensionTransform;

        private int LayerMaskNum;

        private WheelCollider wheelCollider;

        private Vector3 savedSusPos;

        public Gear(ConfigNode n) {
            if (n.HasValue("wheelName"))
                wheelName = n.GetValue("wheelName");
            if (n.HasValue("suspensionParentName"))
                suspensionParentName = n.GetValue("suspensionParentName");
            if (n.HasValue("BrakeTorque"))
                float.TryParse(n.GetValue("BrakeTorque"), out BrakeTorque);
            if (n.HasValue("BrakeSpeed"))
                float.TryParse(n.GetValue("BrakeSpeed"), out BrakeSpeed);
        }

        public void Start(Part part) {
            LayerMaskNum = 1 << (LayerMask.NameToLayer("Local Scenery") & 31) | 1 << (LayerMask.NameToLayer("Default") & 31);
            suspensionTransform = part.FindModelTransform(suspensionParentName);
            wheelTransform = suspensionTransform.findInHierarchy(wheelName);
            wheelCollider = suspensionTransform.parent.findWheelColliderInHierarchy();
            savedSusPos = suspensionTransform.localPosition;
        }

        public void ResetSuspension() {
            suspensionTransform.localPosition = savedSusPos;
        }

        public void SuspensionUpdate() {
            RaycastHit raycastHit;
            if (!Physics.Raycast(this.wheelCollider.transform.position, -this.wheelCollider.transform.up, out raycastHit, this.wheelCollider.suspensionDistance + this.wheelCollider.radius, this.LayerMaskNum)) {
                this.suspensionTransform.position = this.wheelCollider.transform.position - (this.wheelCollider.transform.up * this.wheelCollider.suspensionDistance);
            }
            if (!wheelCollider.enabled) {
                wheelCollider.enabled = true;
            }
            wheelTransform.Rotate(wheelCollider.transform.right, this.wheelCollider.rpm * Time.deltaTime, Space.World);
        }

        private float PrevBrakeTorque = 0, CurBrakeTorque = 0;
        public void UpdateBrakes(bool brakes) {
            if (this.wheelCollider != null) {
                if (!brakes) {
                    CurBrakeTorque = 0f;
                }
                else {
                    CurBrakeTorque = BrakeTorque;
                }
                PrevBrakeTorque = wheelCollider.brakeTorque;
                wheelCollider.brakeTorque = Mathf.Lerp(PrevBrakeTorque, CurBrakeTorque, TimeWarp.deltaTime * BrakeSpeed);
            }
        }

    }
}
