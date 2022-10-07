// Tests implemented by Ronald Foster for CS 3500, September 2022

using SpreadsheetUtilities;

namespace DependencyGraphTests
{
    /// <summary>
    /// Test class for DependencyGraph with all my unit tests
    /// </summary>
    [TestClass]
    public class UnitTest_DependencyGraph
    {
        // ------------Empty Graph Tests------------

        [TestMethod]
        public void EmptyGraphHasSizeZero()
        {
            DependencyGraph dg = new();
            Assert.AreEqual(0, dg.Size);
        }

        [TestMethod]
        public void EmptyGraphHasDependencySizeZero()
        {
            DependencyGraph dg = new();
            Assert.AreEqual(0, dg["a"]);
        }

        [TestMethod]
        public void EmptyGraphHasNoDependents()
        {
            DependencyGraph dg = new();
            Assert.AreEqual(false, dg.HasDependents("a"));
        }

        [TestMethod]
        public void EmptyGraphHasNoDependees()
        {
            DependencyGraph dg = new();
            Assert.AreEqual(false, dg.HasDependees("a"));
        }

        [TestMethod]
        public void EmptyGraphHasEmptyDependentsEnumerable()
        {
            DependencyGraph dg = new();
            string enumeratorCount = "";
            foreach (string str in dg.GetDependents("a"))
            {
                enumeratorCount += str;
            }
            Assert.AreEqual(0, enumeratorCount.Length);
        }

        [TestMethod]
        public void EmptyGraphHasEmptyDependeesEnumerable()
        {
            DependencyGraph dg = new();
            string enumeratorCount = "";
            foreach (string str in dg.GetDependees("a"))
            {
                enumeratorCount += str;
            }
            Assert.AreEqual(0, enumeratorCount.Length);
        }

        [TestMethod]
        public void EmptyGraphIncreasesInSizeWhen_AddDependency()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "b");
            Assert.AreEqual(1, dg.Size);
        }

        [TestMethod]
        public void EmptyGraphDependeeIncreasesInSizeWhen_AddDependency()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "b");
            Assert.AreEqual(1, dg["b"]);
        }

        [TestMethod]
        public void EmptyGraphHasDependentAfter_AddDependency()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "b");
            Assert.AreEqual(true, dg.HasDependents("a"));
        }

        [TestMethod]
        public void EmptyGraphHasDependeeAfter_AddDependency()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "b");
            Assert.AreEqual(true, dg.HasDependees("b"));
        }

        [TestMethod]
        public void EmptyGraphDoesntChangeSizeWhen_RemoveDependency()
        {
            DependencyGraph dg = new();
            dg.RemoveDependency("s", "t");
            Assert.AreEqual(0, dg.Size);
        }

        [TestMethod]
        public void EmptyGraphUpdatesSizeWhen_ReplaceDependents()
        {
            DependencyGraph dg = new();
            HashSet<string> newDependents = new();
            newDependents.Add("a");
            newDependents.Add("b");
            newDependents.Add("c");
            dg.ReplaceDependents("d", newDependents);
            Assert.AreEqual(3, dg.Size);
        }

        [TestMethod]
        public void EmptyGraphAddsAllNewPairsWhen_ReplaceDependents()
        {
            DependencyGraph dg = new();
            Stack<string> newDependents = new();
            newDependents.Push("a");
            newDependents.Push("b");
            newDependents.Push("c");
            dg.ReplaceDependents("d", newDependents);
            bool isEqual = true;
            foreach (string dependent in newDependents)
            {
                foreach (string dependee in dg.GetDependees(dependent))
                {
                    if (dependee != "d")
                    {
                        isEqual = false;
                    }
                }
            }
            foreach (string dependent in dg.GetDependents("d"))
            {
                if (dependent != newDependents.Pop())
                {
                    isEqual = false;
                }
            }
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        public void EmptyGraphHasRightSizeWhen_ReplaceDependees()
        {
            DependencyGraph dg = new();
            HashSet<string> newDependees = new();
            newDependees.Add("a");
            newDependees.Add("b");
            newDependees.Add("c");
            dg.ReplaceDependents("d", newDependees);
            Assert.AreEqual(3, dg.Size);
        }

        [TestMethod]
        public void EmptyGraphAddsAllNewPairsWhen_ReplaceDependees()
        {
            DependencyGraph dg = new();
            Stack<string> newDependees = new();
            newDependees.Push("a");
            newDependees.Push("b");
            newDependees.Push("c");
            dg.ReplaceDependees("d", newDependees);
            bool isEqual = true;
            foreach (string dependee in newDependees)
            {
                foreach (string dependent in dg.GetDependents(dependee))
                {
                    if (dependent != "d")
                    {
                        isEqual = false;
                    }
                }
            }
            foreach (string dependee in dg.GetDependees("d"))
            {
                if (dependee != newDependees.Pop())
                {
                    isEqual = false;
                }
            }
            Assert.IsTrue(isEqual);
        }

        // ------------Simple non-empty graph tests------------

        [TestMethod]
        public void SimpleGraphGetsTheRightDependents()
        {
            DependencyGraph dg = new();
            dg.AddDependency("e", "a");
            dg.AddDependency("e", "b");
            dg.AddDependency("e", "c");
            dg.AddDependency("e", "d");
            dg.AddDependency("e", "e");
            string dependents = "";
            foreach (string dependent in dg.GetDependents("e"))
            {
                dependents += dependent;
            }
            Assert.AreEqual("abcde", dependents);
        }

        [TestMethod]
        public void SimpleGraphGetsTheRightDependees()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "e");
            dg.AddDependency("b", "e");
            dg.AddDependency("c", "e");
            dg.AddDependency("d", "e");
            dg.AddDependency("e", "e");
            string dependees = "";
            foreach (string dependee in dg.GetDependees("e"))
            {
                dependees += dependee;
            }
            Assert.AreEqual("abcde", dependees);
        }

        [TestMethod]
        public void SimpleGraphHasTheRightSizeAfterMultipleAddsAndRemoves()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "e");     // 1
            dg.AddDependency("b", "e");     // 2
            dg.AddDependency("c", "e");     // 3
            dg.AddDependency("d", "e");     // 4
            dg.AddDependency("e", "e");     // 5
            dg.RemoveDependency("e", "e");  // 4
            dg.RemoveDependency("d", "e");  // 3
            dg.RemoveDependency("e", "e");  // 3
            dg.AddDependency("b", "e");     // 3
            dg.AddDependency("e", "e");     // 4
            Assert.AreEqual(4, dg.Size);
        }

        [TestMethod]
        public void SimpleGraphRemovesUselessValuesFromTheGraoph()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "e");
            dg.AddDependency("b", "e");
            dg.AddDependency("c", "e");
            dg.AddDependency("d", "e");
            dg.AddDependency("e", "e");
            dg.AddDependency("e", "a");
            dg.RemoveDependency("e", "a");
            Assert.AreEqual(0, dg["a"]);
        }

        [TestMethod]
        public void SimpleGraphRemovesRightDependency()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "e");
            dg.AddDependency("b", "e");
            dg.AddDependency("d", "e");
            dg.AddDependency("d", "e");
            dg.AddDependency("e", "e");
            dg.RemoveDependency("e", "e");
            Assert.IsTrue(!dg.HasDependents("e"));
        }

        [TestMethod]
        public void SimpleGraphReplacesRightDependents()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "e");
            dg.AddDependency("b", "e");
            dg.AddDependency("c", "e");
            dg.AddDependency("d", "e");
            dg.AddDependency("e", "e");
            Queue<string> newDependees = new();
            newDependees.Enqueue("a");
            newDependees.Enqueue("b");
            newDependees.Enqueue("c");
            dg.ReplaceDependents("a", newDependees);
            string isEqual = "";
            foreach (string dependent in dg.GetDependents("a"))
            {
                isEqual += dependent;
            }
            Assert.AreEqual("abc", isEqual);
        }

        [TestMethod]
        public void SimpleGraphHasRightSizeAfter_ReplaceDependents()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "e");
            dg.AddDependency("b", "e");
            dg.AddDependency("c", "e");
            dg.AddDependency("d", "e");
            dg.AddDependency("e", "e");
            Stack<string> newDependents = new();
            newDependents.Push("a");
            newDependents.Push("b");
            newDependents.Push("c");
            dg.ReplaceDependents("a", newDependents);
            Assert.AreEqual(7, dg.Size);
        }

        [TestMethod]
        public void SimpleGraphReplacesRightDependees()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "e");
            dg.AddDependency("b", "e");
            dg.AddDependency("c", "e");
            dg.AddDependency("d", "e");
            dg.AddDependency("e", "e");
            Queue<string> newDependees = new();
            newDependees.Enqueue("a");
            newDependees.Enqueue("b");
            newDependees.Enqueue("c");
            dg.ReplaceDependees("e", newDependees);
            string isEqual = "";
            foreach (string dependee in dg.GetDependees("e"))
            {
                isEqual += dependee;
            }
            Assert.AreEqual("abc", isEqual);
        }

        [TestMethod]
        public void SimpleGraphHasRightSizeAfter_ReplaceDependees()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "e");
            dg.AddDependency("b", "e");
            dg.AddDependency("c", "e");
            dg.AddDependency("d", "e");
            dg.AddDependency("e", "e");
            Stack<string> newDependees = new();
            newDependees.Push("a");
            newDependees.Push("b");
            newDependees.Push("c");
            dg.ReplaceDependees("e", newDependees);
            Assert.AreEqual(3, dg.Size);
        }

        // ------------Examples tests------------

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void SimpleEmptyTest()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.AreEqual(0, t.Size);
        }


        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void SimpleEmptyRemoveTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "y");
            Assert.AreEqual(1, t.Size);
            t.RemoveDependency("x", "y");
            Assert.AreEqual(0, t.Size);
        }


        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void EmptyEnumeratorTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "y");
            IEnumerator<string> e1 = t.GetDependees("y").GetEnumerator();
            Assert.IsTrue(e1.MoveNext());
            Assert.AreEqual("x", e1.Current);
            IEnumerator<string> e2 = t.GetDependents("x").GetEnumerator();
            Assert.IsTrue(e2.MoveNext());
            Assert.AreEqual("y", e2.Current);
            t.RemoveDependency("x", "y");
            Assert.IsFalse(t.GetDependees("y").GetEnumerator().MoveNext());
            Assert.IsFalse(t.GetDependents("x").GetEnumerator().MoveNext());
        }


        /// <summary>
        ///Replace on an empty DG shouldn't fail
        ///</summary>
        [TestMethod()]
        public void SimpleReplaceTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "y");
            Assert.AreEqual(t.Size, 1);
            t.RemoveDependency("x", "y");
            t.ReplaceDependents("x", new HashSet<string>());
            t.ReplaceDependees("y", new HashSet<string>());
        }



        ///<summary>
        ///It should be possibe to have more than one DG at a time.
        ///</summary>
        [TestMethod()]
        public void StaticTest()
        {
            DependencyGraph t1 = new DependencyGraph();
            DependencyGraph t2 = new DependencyGraph();
            t1.AddDependency("x", "y");
            Assert.AreEqual(1, t1.Size);
            Assert.AreEqual(0, t2.Size);
        }




        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void SizeTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("c", "b");
            t.AddDependency("b", "d");
            Assert.AreEqual(4, t.Size);
        }


        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void EnumeratorTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("c", "b");
            t.AddDependency("b", "d");

            IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
            Assert.IsFalse(e.MoveNext());

            e = t.GetDependees("b").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            String s1 = e.Current;
            Assert.IsTrue(e.MoveNext());
            String s2 = e.Current;
            Assert.IsFalse(e.MoveNext());
            Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));

            e = t.GetDependees("c").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("a", e.Current);
            Assert.IsFalse(e.MoveNext());

            e = t.GetDependees("d").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("b", e.Current);
            Assert.IsFalse(e.MoveNext());
        }




        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void ReplaceThenEnumerate()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "b");
            t.AddDependency("a", "z");
            t.ReplaceDependents("b", new HashSet<string>());
            t.AddDependency("y", "b");
            t.ReplaceDependents("a", new HashSet<string>() { "c" });
            t.AddDependency("w", "d");
            t.ReplaceDependees("b", new HashSet<string>() { "a", "c" });
            t.ReplaceDependees("d", new HashSet<string>() { "b" });

            IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
            Assert.IsFalse(e.MoveNext());

            e = t.GetDependees("b").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            String s1 = e.Current;
            Assert.IsTrue(e.MoveNext());
            String s2 = e.Current;
            Assert.IsFalse(e.MoveNext());
            Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));

            e = t.GetDependees("c").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("a", e.Current);
            Assert.IsFalse(e.MoveNext());

            e = t.GetDependees("d").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("b", e.Current);
            Assert.IsFalse(e.MoveNext());
        }



        /// <summary>
        ///Using lots of data
        ///</summary>
        [TestMethod()]
        public void StressTest()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            // A bunch of strings to use
            const int SIZE = 200;
            string[] letters = new string[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                letters[i] = ("" + (char)('a' + i));
            }

            // The correct answers
            HashSet<string>[] dents = new HashSet<string>[SIZE];
            HashSet<string>[] dees = new HashSet<string>[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                dents[i] = new HashSet<string>();
                dees[i] = new HashSet<string>();
            }

            // Add a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j++)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 4; j < SIZE; j += 4)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Add some back
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j += 2)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove some more
            for (int i = 0; i < SIZE; i += 2)
            {
                for (int j = i + 3; j < SIZE; j += 3)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Make sure everything is right
            for (int i = 0; i < SIZE; i++)
            {
                Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
                Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
            }
        }

        // ------------null argument tests------------

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullArgument_DependeesSize()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "e");
            dg.AddDependency("b", "e");
            dg.AddDependency("c", "e");
            dg.AddDependency("d", "e");
            int fail = dg[null];
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullArgument_HasDependents()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "e");
            dg.AddDependency("b", "e");
            dg.AddDependency("c", "e");
            dg.AddDependency("d", "e");
            bool fail = dg.HasDependents(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullArgument_HasDependees()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "e");
            dg.AddDependency("b", "e");
            dg.AddDependency("c", "e");
            dg.AddDependency("d", "e");
            bool fail = dg.HasDependees(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullArgument_GetDependents()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "e");
            dg.AddDependency("b", "e");
            dg.AddDependency("c", "e");
            dg.AddDependency("d", "e");
            IEnumerable<string> fail = dg.GetDependents(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullArgument_GetDependees()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "e");
            dg.AddDependency("b", "e");
            dg.AddDependency("c", "e");
            dg.AddDependency("d", "e");
            IEnumerable<string> fail = dg.GetDependees(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullArgument_AddDependency()
        {
            DependencyGraph dg = new();
            dg.AddDependency(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullArgument_RemoveDependency()
        {
            DependencyGraph dg = new();
            dg.RemoveDependency(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullArgument_ReplaceDependents()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "e");
            dg.AddDependency("b", "e");
            dg.AddDependency("c", "e");
            dg.AddDependency("d", "e");
            dg.ReplaceDependents(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullArgument_ReplaceDependees()
        {
            DependencyGraph dg = new();
            dg.AddDependency("a", "e");
            dg.AddDependency("b", "e");
            dg.AddDependency("c", "e");
            dg.AddDependency("d", "e");
            dg.ReplaceDependees(null, null);
        }
    }
}