/*
 * MIT License
 *
 * Copyright (c) 2020 Tom Weiland
 * Copyright (c) 2022 Vincent Dowling
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Scripts.CustomServer
{
    public static class ThreadManager
    {
        private static readonly List<Action> ActionsForMainThread = new();
        private static readonly List<Action> ExecuteCopiedOnMainThread = new();
        private static bool _actionToExecuteOnMainThread;

        private const int TicksPerSec = 40;
        private const float MsPerTick = 1000f / TicksPerSec;
        private static bool _isRunning;

        private static readonly Thread MainThreadObject = new(MainThread);


        public static void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            MainThreadObject.Start();

        }

        public static void Stop()
        {
            _isRunning = false;
        }
        

        /// <summary>Sets an action to be executed on the main thread.</summary>
        /// <param name="action">The action to be executed on the main thread.</param>
        public static void ExecuteOnMainThread(Action action)
        {
            if (action == null)
            {
                Debug.Log("No action to execute on main thread!");
                return;
            }

            lock (ActionsForMainThread)
            {
                ActionsForMainThread.Add(action);
                _actionToExecuteOnMainThread = true;
            }
        }
        
        
        /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
        public static void UpdateMain()
        {
            if (!_actionToExecuteOnMainThread) return;
            ExecuteCopiedOnMainThread.Clear();
            lock (ActionsForMainThread)
            {
                ExecuteCopiedOnMainThread.AddRange(ActionsForMainThread);
                ActionsForMainThread.Clear();
                _actionToExecuteOnMainThread = false;
            }

            foreach (var t in ExecuteCopiedOnMainThread)
            {
                t();
            }
        }
        
    
        private static void MainThread()
        {
            Debug.Log($"Main thread started. Running at {TicksPerSec} ticks per second.");
            var nextLoop = DateTime.Now;

            while (_isRunning)
            {
                while (nextLoop < DateTime.Now)
                {
                    UpdateMain();
                    
                    nextLoop = nextLoop.AddMilliseconds(MsPerTick);

                    if (nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}