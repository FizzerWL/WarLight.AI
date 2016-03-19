using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public static class RandomUtility
    {
        [ThreadStatic]
        static private Random _rndInstance;


        public static Random RandomGenerator
        {
            get
            {
                if (_rndInstance == null)
                    _rndInstance = new Random();
                return _rndInstance;
            }
        }

        public static T WeightedRandom<T>(this IEnumerable<T> array, Func<T, double> weightSelector)
        {
            double sum = array.Select(weightSelector).Sum();
            var val = RandomPercentage() * sum;
            foreach (var item in array)
            {
                val -= weightSelector(item);
                if (val <= 0)
                    return item;
            }

            //We should only get here if all weights are 0
            return array.Random();
        }
        public static int WeightedRandomIndex<T>(this List<T> array, Func<T, double> weightSelector)
        {
            double sum = array.Select(weightSelector).Sum();
            var val = RandomPercentage() * sum;
            
            for(int i=0;i<array.Count;i++)
            {
                var item = array[i];
                val -= weightSelector(item);
                if (val <= 0)
                    return i;
            }

            //We should only get here if all weights are 0
            return RandomNumber(array.Count);
        }

        public static IEnumerable<T> OrderByRandom<T>(this IEnumerable<T> items)
        {
            var list = items.ToList();
            list.RandomizeOrder();
            return list;
        }


        public static void RandomizeOrder<T>(this List<T> array)
        {
            /*
            var pairs = new List<KeyValuePair<T, int>>(array.Count);

            foreach (T t in array)
                pairs.Add(new KeyValuePair<T, int>(t, RandomUtility.RandomNumber(int.MaxValue)));

            pairs.Sort((f, s) => f.Value.CompareTo(s.Value));

            for (int i = 0; i < pairs.Count; i++)
                array[i] = pairs[i].Key;*/

            //var copy = new List<T>(array);
            var copy = array.ToList();

            for (int i = 0; i < array.Count; i++)
            {
                var index = RandomNumber(copy.Count);
                array[i] = copy[index];
                copy.RemoveAt(index);
            }
        }

        public static byte RandomByte(int max = 256)
        {
#if FAKERANDOM
            return 144;
#else
            return (byte)RandomNumber(max);
#endif
        }


        static public int RandomNumber(int max)
        {
#if FAKERANDOM
            return 931284 % max;
#else
            return RandomGenerator.Next(max);
#endif
        }


        static public double RandomPercentage()
        {
#if FAKERANDOM
            return 0.444;
#else
            return RandomGenerator.NextDouble();
#endif
        }

        public static double BellRandom(double min, double max)
        {
            var range = max - min;
            var halfRange = range / 2.0;
            var r = RandomPercentage() * halfRange + RandomPercentage() * halfRange;
            return r + min;
        }

        public static KeyValuePair<Y, T> PickRandom<Y, T>(Dictionary<Y, T> distributeTo)
        {
            return distributeTo.ElementAt(RandomNumber(distributeTo.Count));
        }

        public static KeyValuePair<Y, T> ExtractRandomFromDictionary<Y, T>(Dictionary<Y, T> distributeTo)
        {
            KeyValuePair<Y, T> pair = PickRandom(distributeTo);
            distributeTo.Remove(pair.Key);
            return pair;
        }

        public static T ExtractRandomFromListWhere<T>(this List<T> list, Func<T, bool> match)
        {
            var entry = list.RandomWhere(match);
            list.RemoveOne(entry);
            return entry;
        }

        public static T ExtractRandomFromList<T>(this List<T> list)
        {
            Assert.Fatal(list.Count > 0, "Empty list");
            int index = RandomUtility.RandomNumber(list.Count);
            T t = list[index];
            list.RemoveAt(index);
            return t;
        }

        public static T ExtractRandomFromHashSet<T>(this HashSet<T> list)
        {
            Assert.Fatal(list.Count > 0, "Empty list");
            int index = RandomUtility.RandomNumber(list.Count);
            T t = list.ElementAt(index);
            bool ret = list.Remove(t);
            Assert.Fatal(ret);
            return t;
        }

        public static T Random<T>(this IEnumerable<T> array)
        {
            int count = array.Count();
            Assert.Fatal(count > 0, "Empty array passed to Random()");
            return array.ElementAt(RandomUtility.RandomNumber(count));
        }
        public static T RandomWhere<T>(this IEnumerable<T> array, Func<T, bool> match)
        {
            var array2 = array.Where(match);
            int count = array2.Count();
            Assert.Fatal(count > 0, "Empty array passed to Random()");
            return array2.ElementAt(RandomUtility.RandomNumber(count));
        }


        public static T RandomOrDefault<T>(this IEnumerable<T> array, Func<T, bool> match) where T : class
        {
            var list = array.Where(match).ToList();

            if (list.Count == 0)
                return null;
            return list[RandomUtility.RandomNumber(list.Count)];
        }

        public static int RandomNumberBetween(int p1, int p2)
        {
            var rp1 = p1;
            var rp2 = p2;

            if (p2 < p1)
            {
                var t = p1;
                rp1 = p2;
                rp2 = t;
            }

            return RandomNumber(rp2 - rp1) + rp1;
        }
    }
}
