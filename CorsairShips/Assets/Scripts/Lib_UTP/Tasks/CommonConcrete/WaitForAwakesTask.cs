using System.Collections;
using PestelLib.TaskQueueLib;
using PestelLib.Utils;
using UnityDI;

public class WaitForAwakesTask : Task {
    [Dependency]
    private readonly UnityEventsProvider _EventProvider;

    public override void Run() {
        _EventProvider.StartCoroutine(WaitRoutine());
    }

    private IEnumerator WaitRoutine() {
        yield return null;
        OnComplete.Invoke(this);
    }
}
