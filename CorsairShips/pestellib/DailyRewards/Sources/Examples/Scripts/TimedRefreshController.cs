using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityDI;

namespace PestelLib.DailyRewards
{
    public class TimedRefreshController : MonoBehaviour
    {
        [Dependency] UiDailyRewards _rewards;
        [SerializeField] SharedTimeHack _time;

        private void Start()
        {
            ContainerHolder.Container.BuildUp(this);
            _time.OnChange += () => _rewards.LoadRewards();
        }
    }
}
