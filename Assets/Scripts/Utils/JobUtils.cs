using Unity.Jobs;
using UnityEngine;

public static class JobUtils
{
    public static void ScheduleJobAndExecuteParallelFor<T>(T job, int repeatCount, bool enable) where T: struct, IJobFor
    {
        if (!enable) return;
        
        JobHandle handle = default;
        handle = job.ScheduleParallelByRef(repeatCount, 128, handle);
        handle.Complete();
    }
}
