// Andrew Fryzel

using System;
using System.Collections.Generic;
 
namespace Dependencies
{
    /// <summary>
    /// A DependencyGraph can be modeled as a set of dependencies, where a dependency is an ordered 
    /// pair of strings.  Two dependencies (s1,t1) and (s2,t2) are considered equal if and only if 
    /// s1 equals s2 and t1 equals t2.
  
    /// </summary>
    /// 

    public class DependencyGraph
    {
        // Private Instance Variables

        // Dependents and Dependees represented by using seperate Dictionary objects.
        private Dictionary<String, HashSet<string>> dependents;
        private Dictionary<String, HashSet<string>> dependees;

        // Records the size of the Dependency Graph.
        private int size;

        /// <summary>
        /// Creates a DependencyGraph containing no dependencies.
        /// </summary>
        /// 
        public DependencyGraph()
        {
            dependents = new Dictionary<string, HashSet<string>>();
            dependees = new Dictionary<string, HashSet<string>>();

            size = 0;
        }

        /// <summary>
        /// A DependencyGraph can be modeled as a set of dependencies, where a dependency is an ordered 
        /// pair of strings.
        /// Creates a copy of the DependencyGraph dg.
        /// Throws an ArgumentNullException if dg is null.
        /// </summary>
        /// <param name="dg"></param>
        public DependencyGraph(DependencyGraph dg)
        {

            dependents = new Dictionary<string, HashSet<string>>();
            dependees = new Dictionary<string, HashSet<string>>();

            if (dg == null)
            {
                throw new ArgumentNullException();
            }
            // Loop through all dg dependents and add to dependents.
            foreach (String t in dg.dependents.Keys)
            {
                dependents[t] = new HashSet<string>(dg.dependents[t]);
            }

            // Loop through all dg dependees and add to dependees.
            foreach (String t in dg.dependees.Keys)
            {
                dependees[t] = new HashSet<string>(dg.dependees[t]);
            }

            // Update size to be size of dg.
            size = dg.size;
        }

        /// <summary>
        /// The number of dependencies in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get { return size; }
        }

        /// <summary>
        /// Reports whether dependents(s) is non-empty.  Requires s != null.
        /// Throws an ArgumentNullException if s == null.
        /// </summary>
        public bool HasDependents(string s)
        {
            // Null check
            if (s == null)
            {
                throw new ArgumentNullException();
            }

            // Returns true if the dependee has a dependent,
            // else returns false.
            if (dependees.ContainsKey(s))
            {
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Reports whether dependees(s) is non-empty.  Requires s != null.
        /// Throws an ArgumentNullException if s == null.
        /// </summary>
        public bool HasDependees(string s)
        {
            // Null check
            if (s == null)
            {
                throw new ArgumentNullException();
            }

            // Returns true if the dependee has a dependent.
            if (dependents.ContainsKey(s))
            {
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Enumerates dependents(s).  Requires s != null.
        /// Throws an ArgumentNullException if s == null.
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            // Null check
            if (s == null)
            {
                throw new ArgumentNullException();
            }

            // Returns all the dependents of the dependee, 
            // else returns an empty IEnumerable object.
            else if (dependees.ContainsKey(s))
            {
                HashSet<string> returnVal = new HashSet<string>();
                returnVal = dependees[s];

                return returnVal;

            }

            return new HashSet<String>();
        }

        /// <summary>
        /// Enumerates dependees(s).  Requires s != null.
        /// Throws an ArgumentNullException if s == null.
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            // Null check
            if (s == null)
            {
                throw new ArgumentNullException();
            }

            // Returns all the dependees of the dependent, 
            // else returns an empty IEnumerable object.
            if (dependents.ContainsKey(s))
            {
                HashSet<string> returnVal = new HashSet<string>();
                returnVal = dependents[s];

                return returnVal;

            }

            return new HashSet<String>();
        }

        /// <summary>
        /// Adds the dependency (s,t) to this DependencyGraph.
        /// This has no effect if (s,t) already belongs to this DependencyGraph.
        /// Requires s != null and t != null.
        /// Throws an ArgumentNullException if s == null or if t == null.
        /// </summary>
        public void AddDependency(string s, string t)
        {
            // flag to keep track of whether or not size should incremented. 
            // increments if new dependent-dependee relationship is made or if 
            // a new dependent or dependee is added to opposite existing.

            bool flag = false;

            // Null check
            if (s == null || t == null)
            {
                throw new ArgumentNullException();
            }

            // This has no effect if (s,t) already belongs to this DependencyGraph.             
            if (!(dependents.ContainsKey(t) && dependees.ContainsKey(s)))
            {
                flag = true;
            }

            // Check dependents for dependee.
            if (dependents.ContainsKey(t))
            {
                flag = dependents[t].Add(s);
            }

            else if (!dependents.ContainsKey(t))
            {
                // Add dependee.
                HashSet<String> toAdd = new HashSet<string>();
                toAdd.Add(s);
                dependents.Add(t, toAdd);
            }

            // Check dependees for dependents.
            if (dependees.ContainsKey(s))
            {
                flag = dependees[s].Add(t);
            }
            else if (!dependees.ContainsKey(s))
            {
                // Add dependee.
                HashSet<String> toAdd = new HashSet<string>();
                toAdd.Add(t);
                dependees.Add(s, toAdd);
            }

            // Increment size
            if (flag == true)
            {
                size++;
            }
        }

        /// <summary>
        /// Removes the dependency (s,t) from this DependencyGraph.
        /// Does nothing if (s,t) doesn't belong to this DependencyGraph.
        /// Requires s != null and t != null.
        /// Throws an ArgumentNullException if s == null or if t == null.
        /// </summary>
        public void RemoveDependency(string s, string t)
        {
            // Null check
            if (s == null || t == null)
            {
                throw new ArgumentNullException();
            }

            // Check for the dependent/dependee relationship. Removes the dependency.
            if (dependents.ContainsKey(t) && dependees.ContainsKey(s) && dependents[t].Contains(s) && dependees[s].Contains(t))
            {
                dependents[t].Remove(s);
                dependees[s].Remove(t);
                size--;

                // Remove from dependents and dependees if the hashsets are empty after the remove.
                if (dependents[t].Count == 0)
                {
                    dependents.Remove(t);
                }

                if (dependees[s].Count == 0)
                {
                    dependees.Remove(s);
                }
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (s,r).  Then, for each
        /// t in newDependents, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// Throws an ArgumentNullException if s == null or 
        /// when a null string is encountered in the IEnumerable parameters.
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            // Null check
            foreach (String ss in newDependents)
            {
                if (ss == null)
                {
                    throw new ArgumentNullException();
                }
            }

            // Null check
            if (s == null)
            {
                throw new ArgumentNullException();
            }

            // Replace the dependents.
            else
            {
                HashSet<String> existing = new HashSet<String>(GetDependents(s));

                foreach (String item in existing)
                {
                    RemoveDependency(s, item);
                }

                foreach (String item2 in newDependents)
                {
                    AddDependency(s, item2);
                }
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (r,t).  Then, for each 
        /// s in newDependees, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// Throws an ArgumentNullException if t == null or 
        /// when a null string is encountered in the IEnumerable parameters.
        /// </summary>
        public void ReplaceDependees(string t, IEnumerable<string> newDependees)
        {
            // Null check
            foreach (String tt in newDependees)
            {
                if (tt == null)
                {
                    throw new ArgumentNullException();
                }
            }

            // Null check
            if (t == null)
            {
                throw new ArgumentNullException();
            }

            // Replace the dependees.
            else
            {
                HashSet<String> existing = new HashSet<String>(GetDependees(t));

                foreach (String item in existing)
                {
                    RemoveDependency(item, t);
                }

                foreach (String item in newDependees)
                {
                    AddDependency(item, t);
                }
            }
        }
    }
}