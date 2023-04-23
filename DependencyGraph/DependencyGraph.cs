// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)
// Version 1.2 - Daniel Kopta 
//               (Clarified meaning of dependent and dependee.)
//               (Clarified names in solution/project structure.)

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
        // Backing dictionaries of dependents and dependees. A size of pairs as well.
        private Dictionary<string, HashSet<string>> dents;
        private Dictionary<string, HashSet<string>> dees;
        private int size;

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            dents = new Dictionary<string, HashSet<string>>();
            dees = new Dictionary<string, HashSet<string>>();
            size = 0;
        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get { return size; }
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
                // If s exists in the dependees, return its count.
                if (dees.ContainsKey(s))
                    return dees[s].Count;
                else 
                    return 0;
            }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            // If s exists in the dependees, return true if greater than 0.
            if (dents.ContainsKey(s))
            {
                if (dents[s].Count > 0)
                    return true;
                else return false;
            }
            else return false;

        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            // If s exists in the dependees, return true if greater than 0.
            if (dees.ContainsKey(s))
            {
                if (dees[s].Count > 0)
                    return true;
                else return false;
            }
            else return false;
            
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            // If there are no dependents, return empty hashset.
            if (dents.ContainsKey(s))
                return dents[s];
            return new HashSet<string>();
            
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            // If there are no dependents, return empty hashset.
            if (!dees.ContainsKey(s))
                return new HashSet<string>();
            return dees[s];
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

            if (dents.ContainsKey(s))
            {
                if (!dents[s].Contains(t))
                {
                    dents[s].Add(t);
                    size++;
                }
            }
            else
            {
                dents.Add(s, new HashSet<string>() { t });
                size++;
            }

            //// If the key doesnt exist, create a new hashset and add the key value pair.
            //if (!dents.ContainsKey(s))
            //{
            //    dents.Add(s, new HashSet<string>() { t });
            //    size++;
            //}
            //// Otherwise, if the key value pair doesnt already exist, add it.
            //else if (!dents[s].Contains(t))
            //{ 
            //    dents[s].Add(t);
            //    size++;
            //}
            // Do the same with dependees.
            if (dees.ContainsKey(t))
            {
                if (!dees[t].Contains(s))
                    dees[t].Add(s);
            }

            else dees.Add(t, new HashSet<string>() { s });

        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            // If the key exists, remove it from both dictionaries
            // if the key value pair exists.
            if (!dents.ContainsKey(s))
                return;

            if (dees[t].Contains(s))
                dees[t].Remove(s);

            if (dents[s].Contains(t))
                dents[s].Remove(t);

            size--;
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            // If the key exists, do the following:
            if (!dents.ContainsKey(s))
                return;

            // Remove s from each dependee list then remove s.
            foreach (string t in dents[s])
            {
                dees[t].Remove(s);
                size--;
            }


            dents.Remove(s);
            // Add every key value pair from newDependents.
            IEnumerator<string> e = newDependents.GetEnumerator();
            while (e.MoveNext())
                this.AddDependency(s, e.Current);
            
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            // If the key exists, do the following:
            if (dees.ContainsKey(s))
            {
                // Remove s from each dependents list then remove s.
                foreach (string t in dees[s])
                {
                    dents[t].Remove(s);
                    size--;
                }
                dees.Remove(s);

                // Add every key value pair from newDependents.
                
            }
            IEnumerator<string> e = newDependees.GetEnumerator();
            while (e.MoveNext())
                AddDependency(e.Current, s);

        }

    }

}