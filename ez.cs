using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NSTools.Core
{
    public static class EZ
    {
        #region EZQueue implementation
        private static Action ezes, ezesDdol;
        private static float realTime;

        /// <summary>
        /// The interval in seconds from the last EZRunner step to the current one (Read Only). 
        /// </summary>
        public static float deltaTime { get; private set; }

        /// <summary>
        /// EZ Queue factory method.
        /// </summary>
        /// <param name="ddol">Unload on scene change</param>
        /// <returns></returns>
        public static EZQueue Spawn(bool ddol = false) => new EZQueue(ddol);
        public class EZQueue
        {
            
            private class EZTask
            {
                public float duration;
                public Action<float> action;
            }
            
            private Queue<EZTask> queue = new Queue<EZTask>();
            private EZTask currentAction;
            private float startTime = Time.realtimeSinceStartup;
            private bool isLooped;
            /// <summary>
            /// EZ Queue initializer.
            /// </summary>
            /// <param name="ddou">Unload on scene change</param>
            /// <returns></returns>
            public EZQueue(bool ddou = false)
            {
                if (ddou)
                    ezesDdol += Update;
                else
                    ezes += Update;
            }
            
            /// <summary>
            /// Enqueue tweening action
            /// </summary>
            /// <returns>EZQueue instance</returns>
            public EZQueue Tween(float duration, Action<float> action)
            {
                queue.Enqueue(new EZTask{action = action, duration = duration});
                return this;
            }
            
            /// <summary>
            /// Enqueue tweening action
            /// </summary>
            /// <returns>EZQueue instance</returns>
            public EZQueue Tween(Action<float> action) => Tween(0.3f, action);
            
            
            /// <summary>
            /// Enqueue single action call
            /// </summary>
            /// <returns>EZQueue instance</returns>
            public EZQueue Call(Action action) => Tween(0, t => action());
            
            
            /// <summary>
            /// Enqueue delay
            /// </summary>
            /// <param name="duration">Delay duration</param>
            /// <returns>EZQueue instance</returns>
            public EZQueue Delay(float duration = 0.1f) => Tween(duration, t => { });
            
            
            /// <summary>
            /// Enqueue pause until condition is true
            /// </summary>
            /// <returns>EZQueue instance</returns>
            public EZQueue Wait(Func<bool> condition)
            {
                return Tween(float.MaxValue, t =>
                {
                    if (!condition.Invoke()) return;
                    startTime = Time.realtimeSinceStartup;
                    currentAction = null;
                });
            }

            /// <summary>
            /// Loop queue from next action
            /// </summary>
            /// <returns>EZQueue instance</returns>
            public EZQueue Loop() => Call(() => isLooped = true);
            
            
            /// <summary>
            /// Stop EZQueue looping
            /// </summary>
            public void Unloop() => isLooped = false;
            
            /// <summary>
            /// Destroy EZQueue immediately
            /// </summary>
            public void Kill()
            {
                ezes -= Update;
                ezesDdol -= Update;
                queue.Clear();
            }

            private void Update()
            {
                if (currentAction == null)
                {
                    if (queue.Count == 0)
                    {
                        ezes -= Update;
                        ezesDdol -= Update;
                        return;
                    }
                    currentAction = queue.Dequeue();
                }

                var time = realTime - startTime;
                if (time < currentAction.duration)
                {
                    currentAction.action(time / currentAction.duration);
                    return;
                }
                currentAction.action(1);
                startTime += currentAction.duration;
                if (isLooped) 
                    queue.Enqueue(currentAction);
                currentAction = null;
            }
        }
        #endregion
        
        
        #region Global EZ runner
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            SceneManager.sceneUnloaded += s =>  ezes = null;  
            var go = new GameObject("EZRunner");
            go.AddComponent<EZRunner>();
            UnityEngine.Object.DontDestroyOnLoad(go);
        }
        
        [DefaultExecutionOrder(int.MaxValue)]
        private class EZRunner : MonoBehaviour
        {
            private float lastTime;
            void LateUpdate()
            { 
                // Skip tweening to reduce heavy frames
                if (Time.deltaTime > 0.1f) return; 
                
                realTime = Time.realtimeSinceStartup;
                deltaTime = realTime - lastTime;
                lastTime = realTime;
                ezes?.Invoke();
                ezesDdol?.Invoke();
            }
        }
        #endregion


        #region Easing functions
        // Usage: Lerp(from, to, EZ.QuadIn(t))
        
        //Quad
        public static float QuadIn(float t) => t * t;
        public static float QuadOut(float t) => 1 - (1 - t) * (1 - t);
        public static float QuadInOut(float t) => (1 - t) * QuadIn(t) + t * QuadOut(t);
        public static float QuadOutIn(float t) => t * QuadIn(t) + (1 - t) * QuadOut(t);

        //Cubic
        public static float CubicIn(float t) => t * t * t;
        public static float CubicOut(float t) => 1 - (1 - t) * (1 - t) * (1 - t);
        public static float CubicInOut(float t) => (1 - t) * CubicIn(t) + t * CubicOut(t);
        public static float CubicOutIn(float t) => t * CubicIn(t) + (1 - t) * CubicOut(t);

        //Back
        private static float b1 = 1.70158f;
        private static float b2 = 2.70158f;
        public static float BackIn(float t) => b2 * t * t * t - b1 * t * t;
        public static float BackOut(float t) => 1 - BackIn(1 - t);
        
        #endregion
    }
}
