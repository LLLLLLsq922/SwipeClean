using System;
using System.Collections.Generic;
using SwipeClean.Application;
using SwipeClean.Domain;
using SwipeClean.Levels;

internal static class Program
{
    private static int Main()
    {
        try
        {
            CheckStateMachine();
            CheckCleanliness();
            CheckObjectiveAndSession();
            CheckEventBusIsolation();
            Console.WriteLine("SwipeClean pure logic checks passed.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static void CheckStateMachine()
    {
        var machine = new AppStateMachine();
        Require(machine.TryTransition(AppState.Home).Success, "Boot -> Home must succeed.");
        Require(!machine.TryTransition(AppState.Playing).Success, "Home -> Playing must be rejected.");
        Require(machine.TryTransition(AppState.LevelSelect).Success, "Home -> LevelSelect must succeed.");
    }

    private static void CheckCleanliness()
    {
        var samples = new List<StainMassSample>
        {
            new StainMassSample(1f, 0.5f, 1f),
            new StainMassSample(0.5f, 0f, 2f)
        };
        Require(Math.Abs(CleanlinessCalculator.Calculate(samples) - 0.75f) < 0.0001f,
            "Weighted cleanliness must equal 0.75.");
    }

    private static void CheckObjectiveAndSession()
    {
        var now = 10d;
        using var session = new LevelSession("level_01", () => now,
            new ObjectiveEvaluator(0.9f, 0.98f, 0.002f));
        var completed = false;
        session.Completed += result => completed = result.Stars == 2;
        session.Start();
        now = 20d;
        session.RecordCleanliness(0.99f);
        session.RecordCleanliness(0.99f);
        Require(completed, "Two passing snapshots must complete with two stars at 99%.");
    }

    private static void CheckEventBusIsolation()
    {
        var errors = 0;
        var sum = 0;
        var bus = new EventBus(_ => errors++);
        using var bad = bus.Subscribe<int>(_ => throw new InvalidOperationException("expected"));
        using var good = bus.Subscribe<int>(value => sum += value);
        bus.Publish(4);
        Require(errors == 1 && sum == 4, "Event subscriber errors must be isolated.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

