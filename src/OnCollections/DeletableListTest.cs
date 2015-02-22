using System;
using System.Collections.Generic;
using System.Diagnostics;
using Bearded.Utilities.Collections;
using OpenTK;

namespace genericgamedev_tests.OnCollections
{
    static class DeletableListTest
    {
        const int count = 4000;
        const int updates = 10;

        public static void Test()
        {
            Console.WriteLine("Running initial tests, please wait a few seconds...");
            test(() => new LinkedListTestGameState());
            test(() => new DeletableTestGameState(0.2f));

            Console.WriteLine("Running real tests now!");

            realTest();

            Console.WriteLine("done!");
            Console.ReadLine();
        }

        private static void realTest()
        {
            var linkedListTimes = new List<TimeSpan>();
            var deletableTimes = new List<TimeSpan>();

            for (int i = 0; i < 10; i++)
            {
                var t = test(() => new LinkedListTestGameState());

                linkedListTimes.Add(t);
                printTimes(linkedListTimes, deletableTimes);

                t = test(() => new DeletableTestGameState(0.2f));

                deletableTimes.Add(t);
                printTimes(linkedListTimes, deletableTimes);
            }
        }

        private static void printTimes(List<TimeSpan> linkedListTimes, List<TimeSpan> deletableTimes)
        {
            Console.Clear();
            Console.WriteLine("Linked List Times (seconds):");
            var total = TimeSpan.Zero;
            foreach (var time in linkedListTimes)
            {
                Console.WriteLine(time.TotalSeconds);
                total += time;
            }
            var average = total.TotalSeconds / linkedListTimes.Count;
            Console.WriteLine("Average: " + average);

            Console.WriteLine();

            Console.WriteLine("Deletable List Times (seconds):");
            var total2 = TimeSpan.Zero;
            foreach (var time in deletableTimes)
            {
                Console.WriteLine(time.TotalSeconds);
                total2 += time;
            }
            var average2 = total2.TotalSeconds / deletableTimes.Count;
            Console.WriteLine("Average: " + average2);

            Console.WriteLine();

            Console.WriteLine("Deletable List performs ~{0}% slower.",
                (average2 - average) / average * 100);
        }

        static TimeSpan test(Func<IGameState> makeState)
        {
            var timer = Stopwatch.StartNew();

            var state = makeState();

            for (int i = 0; i < updates; i++)
            {
                state.Update();
            }

            return timer.Elapsed;
        }

        interface IGameState
        {
            void Update();
        }

        static int doWork()
        {
            return 0;
            var sillySum = 0;

            for (int i = 0; i < 1000; i++)
            {
                sillySum += i;
            }

            return sillySum;
        }

        #region linked list node


        class LinkedListTestGameState : IGameState
        {
            private readonly MutableLinkedList<LinkedListTestObject> masterList;
            private readonly LinkedList<LinkedListTestObject>[] lists;

            public Random Random { get; private set; }

            public void Add(LinkedListTestObject obj)
            {
                var node0 = this.masterList.Add(obj);
                obj.SetMasterNode(node0);
                var node = this.lists[this.Random.Next(this.lists.Length)].AddLast(obj);
                var node2 = this.lists[this.Random.Next(this.lists.Length)].AddLast(obj);
                obj.SetNodes(node, node2);
            }

            public IEnumerable<LinkedListTestObject> RandomList()
            {
                return this.lists[this.Random.Next(this.lists.Length)];
            }

            public LinkedListTestGameState()
            {
                this.Random = new Random(0);
                this.masterList = new MutableLinkedList<LinkedListTestObject>();
                this.lists = new LinkedList<LinkedListTestObject>[10];
                for (int i = 0; i < 10; i++)
                {
                    this.lists[i] = new LinkedList<LinkedListTestObject>();
                }
                for (int i = 0; i < count; i++)
                {
                    this.Add(new LinkedListTestObject(this));
                }

            }

            public void Update()
            {
                foreach (var obj in this.masterList)
                {
                    obj.Update();
                }
            }
        }

        class LinkedListTestObject : IDeletable
        {
            private readonly LinkedListTestGameState game;
            private readonly Vector3 position;
            private LinkedListNode<LinkedListTestObject> node;
            private MutableLinkedListNode<LinkedListTestObject> masterNode;
            private LinkedListNode<LinkedListTestObject> node2;
            private byte[] data;

            public LinkedListTestObject(LinkedListTestGameState game)
            {
                this.data = new byte[game.Random.Next(10000)];
                this.game = game;
                this.position = new Vector3(
                    (float)game.Random.NextDouble(),
                    (float)game.Random.NextDouble(),
                    (float)game.Random.NextDouble()
                    );
            }

            public void Update()
            {
                var randomList = this.game.RandomList();

                LinkedListTestObject closest = null;
                var closestDistanceSquared = float.PositiveInfinity;
                foreach (var obj in randomList)
                {
                    doWork();

                    var d = (obj.position - this.position).LengthSquared;

                    if (d > closestDistanceSquared)
                        continue;

                    closestDistanceSquared = d;
                    closest = obj;
                }

                if (closest != null)
                {
                    if (this.game.Random.NextDouble() < 0.1)
                    {
                        closest.Delete();
                    }
                }

                if (this.game.Random.NextDouble() < 0.1)
                {
                    this.game.Add(new LinkedListTestObject(this.game));
                }
            }

            public void Delete()
            {
                this.node.List.Remove(this.node);
                this.node2.List.Remove(this.node2);
                this.masterNode.RemoveFromList();
                this.Deleted = true;
            }

            public bool Deleted { get; private set; }

            public void SetNodes(LinkedListNode<LinkedListTestObject> node0, LinkedListNode<LinkedListTestObject> node1)
            {
                this.node = node0;
                this.node2 = node1;
            }

            public void SetMasterNode(MutableLinkedListNode<LinkedListTestObject> node0)
            {
                this.masterNode = node0;
            }
        }


        #endregion

        #region deletable

        class DeletableTestGameState : IGameState
        {
            private readonly DeletableObjectList<DeletableTestObject> masterList;
            private readonly DeletableObjectList<DeletableTestObject>[] lists;

            public Random Random { get; private set; }

            public void Add(DeletableTestObject obj)
            {
                this.masterList.Add(obj);
                this.lists[this.Random.Next(this.lists.Length)].Add(obj);
                this.lists[this.Random.Next(this.lists.Length)].Add(obj);
            }

            public IEnumerable<DeletableTestObject> RandomList()
            {
                return this.lists[this.Random.Next(this.lists.Length)];
            }

            public DeletableTestGameState(float compactPercentage = 0.2f)
            {
                this.Random = new Random(0);
                this.masterList = new DeletableObjectList<DeletableTestObject>
                    { MaxEmptyFraction = compactPercentage };
                this.lists = new DeletableObjectList<DeletableTestObject>[10];
                for (int i = 0; i < 10; i++)
                {
                    this.lists[i] = new DeletableObjectList<DeletableTestObject>
                        { MaxEmptyFraction = compactPercentage };
                }
                for (int i = 0; i < count; i++)
                {
                    this.Add(new DeletableTestObject(this));
                }

            }

            public void Update()
            {
                foreach (var obj in this.masterList)
                {
                    obj.Update();
                }
            }
        }

        class DeletableTestObject : IDeletable
        {
            private readonly DeletableTestGameState game;
            private readonly Vector3 position;
            private byte[] data;

            public DeletableTestObject(DeletableTestGameState game)
            {
                this.data = new byte[game.Random.Next(10000)];
                this.game = game;
                this.position = new Vector3(
                    (float)game.Random.NextDouble(),
                    (float)game.Random.NextDouble(),
                    (float)game.Random.NextDouble()
                    );
            }

            public void Update()
            {
                var randomList = this.game.RandomList();

                DeletableTestObject closest = null;
                var closestDistanceSquared = float.PositiveInfinity;
                foreach (var obj in randomList)
                {
                    doWork();

                    var d = (obj.position - this.position).LengthSquared;

                    if (d > closestDistanceSquared)
                        continue;

                    closestDistanceSquared = d;
                    closest = obj;
                }

                if (closest != null)
                {
                    if (this.game.Random.NextDouble() < 0.1)
                    {
                        closest.Delete();
                    }
                }

                if (this.game.Random.NextDouble() < 0.1)
                {
                    this.game.Add(new DeletableTestObject(this.game));
                }
            }

            public void Delete()
            {
                this.Deleted = true;
            }

            public bool Deleted { get; private set; }
        }

        #endregion
    }
}
