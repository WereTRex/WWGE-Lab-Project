using System;
using System.Collections;
using UnityEngine;
using UnityHFSM;

namespace WwGEProject.AI.Turret
{
    public class TurretAwareState : SubStateMachine<Action>
    {
        public override string Name { get => "Aware"; }


        private readonly TurretController _brain;
        private Coroutine _detectionCoroutine;

        private readonly float _detectionLostTime;
        private float _lastDetectionTime;

        private const float DETECTION_CHECK_DELAY = 0.2f;


        public TurretAwareState(TurretController brain, float detectionLostTime)
        {
            this._brain = brain;

            this._detectionLostTime = detectionLostTime;
        }


        public override void OnEnter()
        {
            base.OnEnter();
            
            if (_detectionCoroutine != null)
                _brain.StopCoroutine(_detectionCoroutine);

            _detectionCoroutine = _brain.StartCoroutine(TryDetection());
            _lastDetectionTime = Time.time;
        }
        public override void OnExit()
        {
            base.OnExit();
            
            if (_detectionCoroutine != null)
                _brain.StopCoroutine(_detectionCoroutine);
        }

        private IEnumerator TryDetection()
        {
            while (true)
            {
                _brain.TryGetTarget();

                if (_brain.Target != null)
                    _lastDetectionTime = Time.time;

                yield return new WaitForSeconds(DETECTION_CHECK_DELAY);
            }
        }



        public override bool CanExit()
        {
            // Check whether enough time has passed from the last detection time for us to transition.
            if (_lastDetectionTime + (Mathf.Max(_detectionLostTime, DETECTION_CHECK_DELAY + 0.1f)) <= Time.time)
            {
                return true;
            }

            return false;
        }
    }
}