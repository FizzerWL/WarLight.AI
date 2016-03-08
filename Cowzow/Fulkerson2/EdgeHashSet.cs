using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI.Cowzow.Fulkerson2
{
    public class EdgeHashSet : IEnumerable<Edge>
    {
        private Dictionary<string, Edge> _edges;

        public EdgeHashSet(IEnumerable<Edge> starting = null)
        {
            if (starting == null)
                _edges = new Dictionary<string, Edge>();
            else
                _edges = starting.ToDictionary(o => o.ID, o => o);
        }

        public void Add(Edge edge)
        {
            _edges[edge.ID] = edge;
        }

        public bool Contains(Edge edge)
        {
            return _edges.ContainsKey(edge.ID);
        }

        public IEnumerator<Edge> GetEnumerator()
        {
            return _edges.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _edges.Values.GetEnumerator();
        }
    }
}
