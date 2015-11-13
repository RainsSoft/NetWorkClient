#if UNITY_WEBPLAYER
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace NetIOCPClient.Core
{
    public enum PerformanceCounterType
    {
        NumberOfItemsHEX32 = 0,
        NumberOfItemsHEX64 = 256,
        NumberOfItems32 = 65536,
        NumberOfItems64 = 65792,
        CounterDelta32 = 4195328,
        CounterDelta64 = 4195584,
        SampleCounter = 4260864,
        CountPerTimeInterval32 = 4523008,
        CountPerTimeInterval64 = 4523264,
        RateOfCountsPerSecond32 = 272696320,
        RateOfCountsPerSecond64 = 272696576,
        RawFraction = 537003008,
        CounterTimer = 541132032,
        Timer100Ns = 542180608,
        SampleFraction = 549585920,
        CounterTimerInverse = 557909248,
        Timer100NsInverse = 558957824,
        CounterMultiTimer = 574686464,
        CounterMultiTimer100Ns = 575735040,
        CounterMultiTimerInverse = 591463680,
        CounterMultiTimer100NsInverse = 592512256,
        AverageTimer32 = 805438464,
        ElapsedTime = 807666944,
        AverageCount64 = 1073874176,
        SampleBase = 1073939457,
        AverageBase = 1073939458,
        RawBase = 1073939459,
        CounterMultiBase = 1107494144,
    }
    public enum PerformanceCounterInstanceLifetime
    {
        Global = 0,
        Process = 1,
    }
    public struct CounterSample
    {

        // do not reorder and keep in sync with the runtime
        // in metadata/mono-perfcounters.c
        private long rawValue;
        private long baseValue;
        private long counterFrequency;
        private long systemFrequency;
        private long timeStamp;
        private long timeStamp100nSec;
        private long counterTimeStamp;
        private PerformanceCounterType counterType;

        public CounterSample(long rawValue,
            long baseValue,
            long counterFrequency,
            long systemFrequency,
            long timeStamp,
            long timeStamp100nSec,
            PerformanceCounterType counterType)
            : this(rawValue, baseValue, counterFrequency,
                systemFrequency, timeStamp, timeStamp100nSec,
                counterType, 0) {
        }

        public CounterSample(long rawValue,
            long baseValue,
            long counterFrequency,
            long systemFrequency,
            long timeStamp,
            long timeStamp100nSec,
            PerformanceCounterType counterType,
            long counterTimeStamp) {
            this.rawValue = rawValue;
            this.baseValue = baseValue;
            this.counterFrequency = counterFrequency;
            this.systemFrequency = systemFrequency;
            this.timeStamp = timeStamp;
            this.timeStamp100nSec = timeStamp100nSec;
            this.counterType = counterType;
            this.counterTimeStamp = counterTimeStamp;
        }

        public static CounterSample Empty = new CounterSample(
            0, 0, 0, 0, 0, 0,
            PerformanceCounterType.NumberOfItems32,
            0);

        public long BaseValue {
            get { return baseValue; }
        }

        public long CounterFrequency {
            get { return counterFrequency; }
        }

        public long CounterTimeStamp {
            get { return counterTimeStamp; }
        }

        public PerformanceCounterType CounterType {
            get { return counterType; }
        }

        public long RawValue {
            get { return rawValue; }
        }

        public long SystemFrequency {
            get { return systemFrequency; }
        }

        public long TimeStamp {
            get { return timeStamp; }
        }

        public long TimeStamp100nSec {
            get { return timeStamp100nSec; }
        }

        public static float Calculate(CounterSample counterSample) {
            return CounterSampleCalculator.ComputeCounterValue(counterSample);
        }

        public static float Calculate(CounterSample counterSample,
            CounterSample nextCounterSample) {
            return CounterSampleCalculator.ComputeCounterValue(counterSample, nextCounterSample);
        }

        public override bool Equals(object obj) {
            if (!(obj is CounterSample))
                return false;
            return Equals((CounterSample)obj);
        }

        public bool Equals(CounterSample other) {
            return
                rawValue == other.rawValue &&
                baseValue == other.counterFrequency &&
                counterFrequency == other.counterFrequency &&
                systemFrequency == other.systemFrequency &&
                timeStamp == other.timeStamp &&
                timeStamp100nSec == other.timeStamp100nSec &&
                counterTimeStamp == other.counterTimeStamp &&
                counterType == other.counterType;
        }

        public static bool operator ==(CounterSample obj1, CounterSample obj2) {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(CounterSample obj1, CounterSample obj2) {
            return !obj1.Equals(obj2);
        }

        public override int GetHashCode() {
            return (int)(rawValue << 28 ^
                (baseValue << 24 ^
                (counterFrequency << 20 ^
                (systemFrequency << 16 ^
                (timeStamp << 8 ^
                (timeStamp100nSec << 4 ^
                (counterTimeStamp ^
                (int)counterType)))))));
        }
    }
    // must be safe for multithreaded operations
    //[InstallerType(typeof(PerformanceCounterInstaller))]
    public sealed class PerformanceCounter : Component, ISupportInitialize
    {

        private string categoryName;
        private string counterName;
        private string instanceName;
        private string machineName;
        IntPtr impl;
        PerformanceCounterType type;
        CounterSample old_sample;
        private bool readOnly;
        bool valid_old;
        bool changed;
        bool is_custom;
        private PerformanceCounterInstanceLifetime lifetime;

        [Obsolete]
        public static int DefaultFileMappingSize = 524288;

        // set catname, countname, instname to "", machname to "."
        public PerformanceCounter() {
            categoryName = counterName = instanceName = "";
            machineName = ".";
        }

        // throws: InvalidOperationException (if catName or countName
        // is ""); ArgumentNullException if either is null
        // sets instName to "", machname to "."
        public PerformanceCounter(String categoryName,
            string counterName)
            : this(categoryName, counterName, false) {
        }

        public PerformanceCounter(string categoryName,
            string counterName,
            bool readOnly)
            : this(categoryName, counterName, "", readOnly) {
        }

        public PerformanceCounter(string categoryName,
            string counterName,
            string instanceName)
            : this(categoryName, counterName, instanceName, false) {
        }

        public PerformanceCounter(string categoryName,
            string counterName,
            string instanceName,
            bool readOnly) {

            if (categoryName == null)
                throw new ArgumentNullException("categoryName");
            if (counterName == null)
                throw new ArgumentNullException("counterName");
            if (instanceName == null)
                throw new ArgumentNullException("instanceName");
            CategoryName = categoryName;
            CounterName = counterName;

            if (categoryName == "" || counterName == "")
                throw new InvalidOperationException();

            InstanceName = instanceName;
            this.instanceName = instanceName;
            this.machineName = ".";
            this.readOnly = readOnly;
            changed = true;
        }

        public PerformanceCounter(string categoryName,
            string counterName,
            string instanceName,
            string machineName)
            : this(categoryName, counterName, instanceName, false) {
            this.machineName = machineName;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        static extern IntPtr GetImpl(string category, string counter,
                string instance, string machine, out PerformanceCounterType ctype, out bool custom);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        static extern bool GetSample(IntPtr impl, bool only_value, out CounterSample sample);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        static extern long UpdateValue(IntPtr impl, bool do_incr, long value);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        static extern void FreeData(IntPtr impl);

        /* the perf counter has changed, ensure it's valid and setup it to
         * be able to collect/update data
         */
        void UpdateInfo() {
            // need to free the previous info
            if (impl != IntPtr.Zero)
                Close();
            impl = GetImpl(categoryName, counterName, instanceName, machineName, out type, out is_custom);
            // system counters are always readonly
            if (!is_custom)
                readOnly = true;
            // invalid counter, need to handle out of mem

            // TODO: reenable this
            //if (impl == IntPtr.Zero)
            //	throw new InvalidOperationException ();
            changed = false;
        }

        // may throw ArgumentNullException
        [DefaultValue(""), ReadOnly(true), RecommendedAsConfigurable(true)]
        [TypeConverter("System.Diagnostics.Design.CategoryValueConverter, " + System.Mono.Consts.AssemblySystem_Design)]
        //[SRDescription("The category name for this performance counter.")]
        public string CategoryName {
            get { return categoryName; }
            set {
                if (value == null)
                    throw new ArgumentNullException("categoryName");
                categoryName = value;
                changed = true;
            }
        }

        // may throw InvalidOperationException
        [MonoTODO]
        [ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("A description describing the counter.")]
        public string CounterHelp {
            get { return ""; }
        }

        // may throw ArgumentNullException
        [DefaultValue(""), ReadOnly(true), RecommendedAsConfigurable(true)]
        [TypeConverter("System.Diagnostics.Design.CounterNameConverter, " + System.Mono.Consts.AssemblySystem_Design)]
        //[SRDescription("The name of this performance counter.")]
        public string CounterName {
            get { return counterName; }
            set {
                if (value == null)
                    throw new ArgumentNullException("counterName");
                counterName = value;
                changed = true;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("The type of the counter.")]
        public PerformanceCounterType CounterType {
            get {
                if (changed)
                    UpdateInfo();
                return type;
            }
        }

        [MonoTODO]
        [DefaultValue(PerformanceCounterInstanceLifetime.Global)]
        public PerformanceCounterInstanceLifetime InstanceLifetime {
            get { return lifetime; }
            set { lifetime = value; }
        }

        [DefaultValue(""), ReadOnly(true), RecommendedAsConfigurable(true)]
        [TypeConverter("System.Diagnostics.Design.InstanceNameConverter, " + System.Mono.Consts.AssemblySystem_Design)]
        //[SRDescription("The instance name for this performance counter.")]
        public string InstanceName {
            get { return instanceName; }
            set {
                if (value == null)
                    throw new ArgumentNullException("value");
                instanceName = value;
                changed = true;
            }
        }

        // may throw ArgumentException if machine name format is wrong
        [MonoTODO("What's the machine name format?")]
        [DefaultValue("."), Browsable(false), RecommendedAsConfigurable(true)]
        //[SRDescription("The machine where this performance counter resides.")]
        public string MachineName {
            get { return machineName; }
            set {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value == "" || value == ".") {
                    machineName = ".";
                    changed = true;
                    return;
                }
                throw new PlatformNotSupportedException();
            }
        }

        // may throw InvalidOperationException, Win32Exception
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [MonitoringDescription("The raw value of the counter.")]
        public long RawValue {
            get {
                CounterSample sample;
                if (changed)
                    UpdateInfo();
                GetSample(impl, true, out sample);
                // should this update old_sample as well?
                return sample.RawValue;
            }
            set {
                if (changed)
                    UpdateInfo();
                if (readOnly)
                    throw new InvalidOperationException();
                UpdateValue(impl, false, value);
            }
        }

        [Browsable(false), DefaultValue(true)]
        [MonitoringDescription("The accessability level of the counter.")]
        public bool ReadOnly {
            get { return readOnly; }
            set { readOnly = value; }
        }

        public void BeginInit() {
            // we likely don't need to do anything significant here
        }

        public void EndInit() {
            // we likely don't need to do anything significant here
        }

        public void Close() {
            IntPtr p = impl;
            impl = IntPtr.Zero;
            if (p != IntPtr.Zero)
                FreeData(p);
        }

        public static void CloseSharedResources() {
            // we likely don't need to do anything significant here
        }

        // may throw InvalidOperationException, Win32Exception
        public long Decrement() {
            return IncrementBy(-1);
        }

        protected override void Dispose(bool disposing) {
            Close();
        }

        // may throw InvalidOperationException, Win32Exception
        public long Increment() {
            return IncrementBy(1);
        }

        // may throw InvalidOperationException, Win32Exception
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public long IncrementBy(long value) {
            if (changed)
                UpdateInfo();
            if (readOnly) {
                // FIXME: This should really throw, but by now set this workaround in place.
                //throw new InvalidOperationException ();
                return 0;
            }
            return UpdateValue(impl, true, value);
        }

        // may throw InvalidOperationException, Win32Exception
        public CounterSample NextSample() {
            CounterSample sample;
            if (changed)
                UpdateInfo();
            GetSample(impl, false, out sample);
            valid_old = true;
            old_sample = sample;
            return sample;
        }

        // may throw InvalidOperationException, Win32Exception
        public float NextValue() {
            CounterSample sample;
            if (changed)
                UpdateInfo();
            GetSample(impl, false, out sample);
            float val;
            if (valid_old)
                val = CounterSampleCalculator.ComputeCounterValue(old_sample, sample);
            else
                val = CounterSampleCalculator.ComputeCounterValue(sample);
            valid_old = true;
            old_sample = sample;
            return val;
        }

        // may throw InvalidOperationException, Win32Exception
        [MonoTODO]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public void RemoveInstance() {
            throw new NotImplementedException();
        }
    }


    public static class CounterSampleCalculator
    {

        public static float ComputeCounterValue(CounterSample newSample) {
            switch (newSample.CounterType) {
                case PerformanceCounterType.RawFraction:
                case PerformanceCounterType.NumberOfItems32:
                case PerformanceCounterType.NumberOfItemsHEX32:
                case PerformanceCounterType.NumberOfItems64:
                case PerformanceCounterType.NumberOfItemsHEX64:
                    return (float)newSample.RawValue;
                default:
                    return 0;
            }
        }

        [MonoTODO("What's the algorithm?")]
        public static float ComputeCounterValue(CounterSample oldSample,
            CounterSample newSample) {
            if (newSample.CounterType != oldSample.CounterType)
                throw new Exception("The counter samples must be of the same type");
            switch (newSample.CounterType) {
                case PerformanceCounterType.RawFraction:
                case PerformanceCounterType.NumberOfItems32:
                case PerformanceCounterType.NumberOfItemsHEX32:
                case PerformanceCounterType.NumberOfItems64:
                case PerformanceCounterType.NumberOfItemsHEX64:
                    return (float)newSample.RawValue;
                case PerformanceCounterType.AverageCount64:
                    return (float)(newSample.RawValue - oldSample.RawValue) / (float)(newSample.BaseValue - oldSample.BaseValue);
                case PerformanceCounterType.AverageTimer32:
                    return (((float)(newSample.RawValue - oldSample.RawValue)) / newSample.SystemFrequency) / (float)(newSample.BaseValue - oldSample.BaseValue);
                case PerformanceCounterType.CounterDelta32:
                case PerformanceCounterType.CounterDelta64:
                    return (float)(newSample.RawValue - oldSample.RawValue);
                case PerformanceCounterType.CounterMultiTimer:
                    return ((float)(newSample.RawValue - oldSample.RawValue)) / (float)(newSample.TimeStamp - oldSample.TimeStamp) * 100.0f / newSample.BaseValue;
                case PerformanceCounterType.CounterMultiTimer100Ns:
                    return ((float)(newSample.RawValue - oldSample.RawValue)) / (float)(newSample.TimeStamp100nSec - oldSample.TimeStamp100nSec) * 100.0f / newSample.BaseValue;
                case PerformanceCounterType.CounterMultiTimerInverse:
                    return (newSample.BaseValue - ((float)(newSample.RawValue - oldSample.RawValue)) / (float)(newSample.TimeStamp - oldSample.TimeStamp)) * 100.0f;
                case PerformanceCounterType.CounterMultiTimer100NsInverse:
                    return (newSample.BaseValue - ((float)(newSample.RawValue - oldSample.RawValue)) / (float)(newSample.TimeStamp100nSec - oldSample.TimeStamp100nSec)) * 100.0f;
                case PerformanceCounterType.CounterTimer:
                case PerformanceCounterType.CountPerTimeInterval32:
                case PerformanceCounterType.CountPerTimeInterval64:
                    return ((float)(newSample.RawValue - oldSample.RawValue)) / (float)(newSample.TimeStamp - oldSample.TimeStamp);
                case PerformanceCounterType.CounterTimerInverse:
                    return (1.0f - ((float)(newSample.RawValue - oldSample.RawValue)) / (float)(newSample.TimeStamp100nSec - oldSample.TimeStamp100nSec)) * 100.0f;
                case PerformanceCounterType.ElapsedTime:
                    // FIXME
                    return 0;
                case PerformanceCounterType.Timer100Ns:
                    return ((float)(newSample.RawValue - oldSample.RawValue)) / (float)(newSample.TimeStamp - oldSample.TimeStamp) * 100.0f;
                case PerformanceCounterType.Timer100NsInverse:
                    return (1f - ((float)(newSample.RawValue - oldSample.RawValue)) / (float)(newSample.TimeStamp - oldSample.TimeStamp)) * 100.0f;
                case PerformanceCounterType.RateOfCountsPerSecond32:
                case PerformanceCounterType.RateOfCountsPerSecond64:
                    return ((float)(newSample.RawValue - oldSample.RawValue)) / (float)(newSample.TimeStamp - oldSample.TimeStamp) * 10000000;
                default:
                    Console.WriteLine("Counter type {0} not handled", newSample.CounterType);
                    return 0;
            }
        }
    }
}



#endif