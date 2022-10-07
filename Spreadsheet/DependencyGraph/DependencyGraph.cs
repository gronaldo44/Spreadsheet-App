// Functionality implemented by Ronald Foster for CS 3500, September 2022

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpreadsheetUtilities
{

    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// t1 depends on s1; s1 must be evaluated before t1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        (The set of things that depend on s)    
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///        (The set of things that s depends on) 
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        private Dictionary<string, HashSet<string>> dependents;     // dependee -> { dependents }
        private Dictionary<string, HashSet<string>> dependees;      // dependent -> { dependees }
        private int p_size;

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            dependents = new();
            dependees = new();
            p_size = 0;
        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get { return p_size; }
        }


        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s]
        {
            get
            {
                if (dependees.ContainsKey(s))
                {
                    return dependees[s].Count;
                }
                else
                {
                    return 0;
                }
            }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            return !dependents.IsEmptyDependency(s);
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            return !dependees.IsEmptyDependency(s);
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            return dependents.GetDependencies(s);
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            return dependees.GetDependencies(s);
        }


        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   t depends on s
        ///
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param>        /// 
        public void AddDependency(string s, string t)
        {
            if (!dependents.ContainsKey(s))
            {   // this is a new dependent
                AddDependee(s, t);
                HashSet<string> newDependents = new();
                newDependents.Add(t);
                dependents.Add(s, newDependents);
                p_size++;
            }
            else if (!dependents[s].Contains(t))
            {   // the dependent is already in the graph but not with this dependee
                AddDependee(s, t);
                dependents[s].Add(t);
                p_size++;
            }   
        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            if (dependents.ContainsKey(s))
            {
                if (dependents[s].Remove(t))
                {   // t is no longer dependent on s, so we need to update dependees
                    dependees[t].Remove(s);
                    if (dependees.IsEmptyDependency(t))
                    {   // t is no longer a dependent of anything in the graph
                        dependees.Remove(t);
                    }
                    p_size--;
                }
            }   
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            // Remove the old
            foreach (string dependent in GetDependents(s))
            {
                RemoveDependency(s, dependent);
            }
            // Add the new
            foreach (string t in newDependents)
            {
                AddDependency(s, t);
            }
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            // Remove the old
            foreach (string dependee in GetDependees(s))
            {
                RemoveDependency(dependee, s);
            }
            // Add the new
            foreach (string t in newDependees)
            {
                AddDependency(t, s);
            }
        }

        /// <summary>
        /// Adds the dependee to the graph based on its new dependency pair (s, t)
        /// t depends on s
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        private void AddDependee(string s, string t)
        {
            if (!dependees.ContainsKey(t))
            {   // t wasn't already in the graph
                HashSet<string> newDependents = new();
                newDependents.Add(s);
                dependees.Add(t, newDependents);
            }
            else
            {   // t is already in the graph but needs to be dependent on s
                dependees[t].Add(s);
            }
        }

        ///// <summary>
        ///// Useful ToString representation for debugging
        ///// 
        ///// dependents:
        ///// "a": { "b"  "c" }
        ///// "b": { "d" }
        ///// "d": { "d" }
        ///// dependees:
        ///// "b": { "a" }
        ///// "c": { "a" }
        ///// "d": { "b"  "d" }
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public override string ToString()
        //{
        //    string graph = "dependents:\n";
        //    foreach (string dependee in dependents.Keys)
        //    {
        //        graph += "\"" + dependee + "\": {";
        //        foreach (string dependent in GetDependents(dependee))
        //        {
        //            graph += "\"" + dependent + "\"";
        //        }
        //        graph += "}\n";
        //    }
        //    graph += "dependees: \n";
        //    foreach (string dependent in dependees.Keys)
        //    {
        //        graph += "\"" + dependent + "\": {";
        //        foreach (string dependee in GetDependees(dependent))
        //        {
        //            graph += " \"" + dependee + "\" ";
        //        }
        //        graph += "}\n";
        //    }
        //    return graph;
        //}

    }

    internal static class PS2DictionaryExtensions
    {
        /// <summary>
        /// Is this Graph empty at the given key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="d"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsEmptyDependency<T>(this Dictionary<T, HashSet<string>> graph, T key) where T : notnull
        {
            if (graph.ContainsKey(key))
            {
                if (graph[key].Count > 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets a list of dependencies tied to this key on the graph
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="graph"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetDependencies<T>(this Dictionary<T, HashSet<string>> graph, T key) where T : notnull
        {
            if (!graph.IsEmptyDependency(key))
            {
                return graph[key];
            }
            else
            {
                return new HashSet<string>();
            }
        }
    }

}