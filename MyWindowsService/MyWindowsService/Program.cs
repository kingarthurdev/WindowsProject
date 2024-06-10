using System;
using System.ServiceProcess;
using projDependency;

namespace MyWindowsService
{
    public class MyService : ServiceBase
    {
        private DependencyClass dependency;

        public MyService()
        {
            ServiceName = "MyService";
        }

        protected override void OnStart(string[] args)
        {
            dependency = new DependencyClass();
            dependency.DoSomething();
        }

        protected override void OnStop()
        {
            dependency = null;
        }
    }
}
