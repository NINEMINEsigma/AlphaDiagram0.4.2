using System.Collections.Generic;
using AD.BASE;
using UnityEngine;

namespace AD.Utility
{
    public static class TimeExtension
    {
        public static Dictionary<string,TimeClocker> TimeClockers = new();

        public static TimeClocker GetTimer()
        {
            return new();
        }

        public static TimeClocker RegistTimer(string name, out TimeClocker last)
        {
            last = TimeClockers.ContainsKey(name) ? TimeClockers[name] : null;
            TimeClockers[name] = new();
            return TimeClockers[name];
        }

        public static bool UnRegistTimer(string name)
        {
            return TimeClockers.Remove(name);
        }

        public static void Clear()
        {
            TimeClockers.Clear();
        }
    }

    //using ms
    public class TimeClocker:ICanInitialize
    {
        long clockS, clockE,clockU;

        internal TimeClocker() 
        {
            Init();
        }

        public void Init()
        {
            clockE = clockS = System.DateTime.Now.Ticks;
            clockU = 0;
        }

        public void Update()
        {
            clockU = System.DateTime.Now.Ticks - clockE;
            clockE = System.DateTime.Now.Ticks;
        }

        public float StartTime
        {
            get => clockS / 10000.0f;
        }
        public float LastUpdateTime
        {
            get => clockE / 10000.0f;
        }
        public float LastDalteTime
        {
            get => clockU / 10000.0f;
        }
        public float KeepingTime
        {
            get => (clockE - clockS) / 10000.0f;
        }
    }
}
