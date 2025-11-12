using UnityEngine;
using System.Collections;

namespace BikiniNoir
{
    public class SlowMotion : MonoBehaviour
    {
        public float slowTimeAllowed = 2.0f;
        public float slowMotionScale = 0.3f;
        public bool overrideSlowTimeAllowed = false;

        private float _currentSlowMo = 0;
        private bool _timeSlowed = false;

        bool TimeSlowed
        {
            get { return _timeSlowed; }
        }

        void Update()
        {
            //// Debug Code
            //if (Input.GetButtonDown("Fire2"))
            //{
            //    StartSlowMotion();
            //}
            //if (Input.GetButtonDown("Jump"))
            //{
            //    StopSlowMotion();
            //}

            // If we are in slowed time
            if (Time.timeScale == slowMotionScale)
            {
                // Start counting the slowMo time
                _currentSlowMo += Time.deltaTime;
            }

            // If slow time has ended
            if (_currentSlowMo > slowTimeAllowed && !overrideSlowTimeAllowed)
            {
                Time.timeScale = 1.0f;
                // Smooth the delta time
                Time.fixedDeltaTime = 0.02f * Time.timeScale;

                _currentSlowMo = 0;
                _timeSlowed = false;
            }
        }

        // Starts Slow Motion
        public void StartSlowMotion()
        {
            if (Time.timeScale == 1.0f)
            {
                Time.timeScale = slowMotionScale;
                _timeSlowed = true;
            }
        }

        // Stops Slow Motion
        public void StopSlowMotion()
        {
            if (Time.timeScale == slowMotionScale)
            {
                Time.timeScale = 1.0f;
                // Smooth the delta time
                Time.fixedDeltaTime = 0.02f * Time.timeScale;

                _currentSlowMo = 0;
                _timeSlowed = false;
            }
        }
    }
}