#region

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#endregion

namespace FailCake
{
    public class util_fade_timer
    {
        #region PRIVATE

        private static readonly Dictionary<string, util_fade_timer> timers = new Dictionary<string, util_fade_timer>();
        private static int ID;

        #region TIMER

        private string _id;
        private float _speed;
        private float _initial;
        private float _targetValue;
        private float _timer;
        private float _currentValue;
        private Action<float> _onTick;
        private Action<float> _onComplete;

        #endregion

        #endregion

        #region CLEANUP - DOMAIN RELOAD

        #if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod]
        private static void InitializeOnLoad() {
            util_fade_timer.ID = 0;
            util_fade_timer.timers.Clear();
        }
        #endif

        #endregion

        public static void Tick(float delta) {
            if (util_fade_timer.timers == null || util_fade_timer.timers.Count <= 0) return;

            foreach (KeyValuePair<string, util_fade_timer> timer in util_fade_timer.timers.ToList())
            {
                #if !UNITY_EDITOR && !DEVELOPMENT_BUILD
                try
                {
                    #endif
                if (timer.Value != null)
                    timer.Value.tick(delta);
                else
                    util_fade_timer.timers.Remove(timer.Key);
                #if !UNITY_EDITOR && !DEVELOPMENT_BUILD
                }
                catch (Exception err)
                {
                    Debug.LogError(err.Message);
                    util_fade_timer.timers.Remove(timer.Key);
                }
                #endif
            }
        }

        public static util_fade_timer Fade(float speed, float initial, float target, Action<float> onTick, Action<float> onComplete = null) {
            util_fade_timer t = new util_fade_timer {
                _speed = speed,
                _initial = initial,
                _targetValue = target,
                _timer = 0f,
                _onTick = onTick,
                _onComplete = onComplete,
                _id = (util_fade_timer.ID++).ToString()
            };

            t.Start();
            return t;
        }

        public static void Clear() {
            foreach (util_fade_timer timer in util_fade_timer.timers.Values.ToList())
                if (timer != null)
                    timer.Stop();

            util_fade_timer.timers.Clear();
            util_fade_timer.ID = 0;
        }

        public void Stop() {
            util_fade_timer.timers.Remove(this._id);
        }

        public void Start() {
            if (util_fade_timer.timers.ContainsKey(this._id)) throw new UnityException("Fade already started");
            util_fade_timer.timers.Add(this._id, this);
        }

        #region PRIVATE

        private void tick(float delta) {
            this._timer += this._speed * delta;
            this._currentValue = Mathf.Lerp(this._initial, this._targetValue, this._timer);

            if (this._onTick != null) this._onTick.Invoke(this._currentValue);

            if (this._timer >= 1f)
            {
                this.Stop();
                if (this._onComplete != null) this._onComplete.Invoke(this._targetValue);
            }
        }

        #endregion

        #if DEVELOPMENT_BUILD || UNITY_EDITOR
        public static string DEBUG() {
            string data = "\n--------------- ACTIVE FADE TIMERS: " + util_fade_timer.timers.Count + " | ID : " + util_fade_timer.ID;
            data += "\nCURRENT ID: " + util_fade_timer.ID;

            foreach (util_fade_timer timer in util_fade_timer.timers.Values) data += "\n [" + timer._id + "] VALUE: " + timer._currentValue + " | TARGET: " + timer._targetValue + " | SPEED: " + timer._speed;

            return data;
        }
        #endif
    }
}

/*
# MIT License Copyright (c) 2024 FailCake

# Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the
# "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish,
# distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to
# the following conditions:
#
# The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
# MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR
# ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
# SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/