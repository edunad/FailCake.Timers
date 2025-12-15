#region

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#endregion

namespace FailCake
{
    public class util_timer
    {
        #region PRIVATE

        public static Dictionary<string, util_timer> timers = new Dictionary<string, util_timer>();

        private static int ID;
        private static float COOLDOWN_ID = -1;

        #region TIMER

        private string _id;
        private float _nextTick;
        private float _delay;
        private int _iterations;
        private Action<int> _func;
        private Action _onComplete;
        private float _pausedTime;
        private bool _paused;
        private bool _infinite;

        #endregion

        #endregion

        #region CLEANUP - DOMAIN RELOAD

        #if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod]
        private static void InitializeOnLoad() {
            util_timer.COOLDOWN_ID = -1;
            util_timer.ID = 0;

            util_timer.timers.Clear();
        }
        #endif

        #endregion

        public static void Tick() {
            // ID RESET ---
            if (util_timer.COOLDOWN_ID != -1 && util_timer.COOLDOWN_ID < Time.time)
            {
                util_timer.COOLDOWN_ID = -1;
                if (util_timer.timers.Count == 0) util_timer.ID = 0;
            }

            // ------------
            if (util_timer.timers == null || util_timer.timers.Count <= 0) return;

            foreach (KeyValuePair<string, util_timer> timer in util_timer.timers.ToList())
            {
                #if !UNITY_EDITOR && !DEVELOPMENT_BUILD
                try
                {
                    #endif
                if (timer.Value != null)
                    timer.Value.InternalTick();
                else
                    util_timer.timers.Remove(timer.Key);
                #if !UNITY_EDITOR && !DEVELOPMENT_BUILD
                }
                catch (Exception err)
                {
                    Debug.LogError(err.StackTrace);
                    util_timer.timers.Remove(timer.Key);
                }
                #endif
            }
        }

        public static util_timer Simple(float delay, Action func, bool start = true) {
            return util_timer.Create(1, delay, _ => { func.Invoke(); }, null, start);
        }

        public static util_timer Create(int reps, float delay, Action<int> func, Action complete = null, bool start = true) {
            util_timer t = new util_timer {
                _iterations = reps,
                _delay = delay,
                _func = func,
                _id = (util_timer.ID++).ToString(),
                _infinite = reps < 0,
                _onComplete = complete,
                _paused = !start
            };

            t.Start();
            return t;
        }

        public static void Clear() {
            foreach (util_timer timer in util_timer.timers.Values.ToList())
                if (timer != null)
                    timer.Stop();

            util_timer.timers.Clear();
            util_timer.ID = 0;
        }

        public bool IsPaused() {
            return this._paused;
        }

        public void SetPaused(bool pause, bool reset = false) {
            if (this._paused == pause) return;
            this._paused = pause;

            if (pause)
                this._pausedTime = Time.time;
            else
            {
                if (reset)
                    this._nextTick = Time.time + this._delay;
                else
                    this._nextTick += Time.time - this._pausedTime;

                this._pausedTime = 0;
            }
        }

        public void SetDelay(float delay) {
            this._delay = delay;
            this._nextTick = Time.time + this._delay;
            this.InternalTick();
        }

        public float GetDelay() {
            return this._delay;
        }

        public void SetTicksLeft(int ticks) {
            this._iterations = Mathf.Max(ticks, 0);
        }

        public int GetTicksLeft() {
            return this._iterations;
        }

        public void Stop() {
            if (!util_timer.timers.Remove(this._id)) return;
            if (util_timer.timers.Count == 0) util_timer.COOLDOWN_ID = Time.time + 8;
        }

        public void Start() {
            if (util_timer.timers.ContainsKey(this._id)) throw new Exception("Timer already started");

            this._nextTick = Time.time + this._delay;
            util_timer.timers.Add(this._id, this);
        }

        #region PRIVATE

        private void InternalTick() {
            float currTime = Time.time;
            if (this._paused || currTime < this._nextTick) return;

            if (!this._infinite)
                this._iterations--;
            else
                this._iterations = (this._iterations + 1) % 10; // Simulate iterations, but limit them

            if (this._func != null) this._func.Invoke(this._iterations);

            if (this._iterations == 0 && !this._infinite)
            {
                if (this._onComplete != null) this._onComplete.Invoke();
                this.Stop();
            }
            else
                this._nextTick = currTime + this._delay;
        }

        #endregion
    }
}

/*# MIT License Copyright (c) 2024 FailCake

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