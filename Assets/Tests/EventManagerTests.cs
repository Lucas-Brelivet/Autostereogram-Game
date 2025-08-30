using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EventManagerTests
{
    private bool test1Success = false;

    private int testSubscriberCount = 0;

    private int test2ReceivedArg;
    private string test2ReceivedArg2;

    private void Test1Action()
    {
        test1Success = true;
    }

    private void Test2Action(int arg)
    {
        test2ReceivedArg = arg;
    }
    private void Test2Action2(string arg)
    {
        test2ReceivedArg2 = arg;
    }

    private void TestSubscriberCountAction1()
    {
        testSubscriberCount++;
    }
    private void TestSubscriberCountAction2()
    {
        testSubscriberCount++;
    }
    private void TestSubscriberCountAction3()
    {
        testSubscriberCount++;
    }

    // A Test behaves as an ordinary method
    [Test]
    public void EventManagerTestsSimplePasses()
    {
        EventManager evtMng = EventManager.Instance;
        Assert.IsNotNull(evtMng);

        evtMng.SubscribeToEvent("Test1", Test1Action);
        evtMng.InvokeEvent("Test1");
        Assert.IsTrue(test1Success);

        test1Success = false;

        evtMng.InvokeEvent<int>("Test1", 4);
        Assert.IsFalse(test1Success);

        evtMng.UnsubscribeFromEvent("Test1", Test1Action );
        evtMng.InvokeEvent("Test1");
        Assert.IsFalse(test1Success);

        evtMng.SubscribeToEvent<int>("Test2", Test2Action);
        evtMng.InvokeEvent<int>("Test2", 3);
        Assert.AreEqual(3, test2ReceivedArg);

        test2ReceivedArg = 0;
        evtMng.SubscribeToEvent<string>("Test2", Test2Action2);
        evtMng.InvokeEvent<string>("Test2", "foo");
        Assert.AreEqual("foo", test2ReceivedArg2);
        Assert.AreEqual(0, test2ReceivedArg);

        test2ReceivedArg2 = null;
        evtMng.InvokeEvent<int>("Test2", 3);
        Assert.AreEqual(0, test2ReceivedArg);
        Assert.IsNull(test2ReceivedArg2);

        evtMng.InvokeEvent("Test2");
        Assert.AreEqual(0, test2ReceivedArg);
        Assert.IsNull(test2ReceivedArg2);

        evtMng.UnsubscribeFromEvent<string>("Test2", Test2Action2);
        evtMng.InvokeEvent<string>("Test2", "foo");
        Assert.IsNull(test2ReceivedArg2);

        evtMng.SubscribeToEvent("TestSubscriberCount", TestSubscriberCountAction1);
        evtMng.SubscribeToEvent("TestSubscriberCount", TestSubscriberCountAction2);
        evtMng.SubscribeToEvent("TestSubscriberCount", TestSubscriberCountAction3);
        evtMng.InvokeEvent("TestSubscriberCount");
        Assert.AreEqual(3, testSubscriberCount);

        testSubscriberCount = 0;
        evtMng.UnsubscribeFromEvent("TestSubscriberCount", TestSubscriberCountAction2);
        evtMng.InvokeEvent("TestSubscriberCount");
        Assert.AreEqual(2, testSubscriberCount);

        testSubscriberCount = 0;
        evtMng.SubscribeToEvent<int>("TestSubscriberCount", i => { testSubscriberCount++; });
        evtMng.InvokeEvent<int>("TestSubscriberCount", 4);
        Assert.AreEqual(1, testSubscriberCount);
    }
}
