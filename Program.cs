#define USE_END
//#undef USE_END

using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Activities.Flowchart.Activities;
using Elsa.Workflows.IncidentStrategies;
using Elsa.Workflows.Memory;
using Elsa.Workflows.Models;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddElsa();
var serviceProvider = services.BuildServiceProvider();

var values = new Variable<string[]>("values", ["one", "two", "three"]).WithWorkflowStorage();
var value = new Variable<string>("value", "hello").WithWorkflowStorage();

var fail = new Fail();
var write = new WriteLine(value);
var end = new End();

var workflow = new Workflow() {
    Root = new ForEach() {
                Items = new(values),
                CurrentValue = new(value),
                Body = new Flowchart() {
                    Activities = {
                        fail,
                        write,
#if USE_END
                        end,
#endif
                    },
                    Connections = {
                        new(fail, write),
#if USE_END
                        new(write, end),
#endif
                    },
                    Start = fail,
                },
            },
    Variables = [values, value],
    Options = new WorkflowOptions() {
        IncidentStrategyType = typeof(ContinueWithIncidentsStrategy),
    }
};

var workflowRunner = serviceProvider.GetRequiredService<IWorkflowRunner>();
await workflowRunner.RunAsync(workflow);

class Fail : Activity {
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context) {
        await context.CompleteActivityAsync();
        throw new Exception();
    }
}

