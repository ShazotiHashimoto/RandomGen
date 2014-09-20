﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RandomGen.Fluent;

namespace RandomGen
{
    class RandomLink : IRandom
    {
        // Single instance of random generator shared by all links
        internal readonly Random Random;

        public RandomLink(Random random)
        {
            this.Random = random;
        }

        public INumbers Numbers { get { return new NumbersLink(this); } }
        public INames Names { get { return new NamesLink(this); } }
        public ITime Time { get { return new TimeLink(this); } }
        public IText Text { get { return new TextLink(this); } }
        public IInternet Internet { get { return new InternetLink(this); } }

        public Func<T> Items<T>(IEnumerable<T> items, IEnumerable<double> weights = null)
        {
            var copy = items.ToList();
            if (weights == null)
            {
                var factory = this.Numbers.Integers(0, copy.Count);
                return () => copy[factory()];
            }
            else
            {
                var weightsCopy = weights.ToList();
                if (weightsCopy.Count != copy.Count)
                    throw new ArgumentException("Weights must have the same number of items as items.");

                var cumSum = new double[weightsCopy.Count];
                cumSum[0] = weightsCopy[0];
                Enumerable.Range(1, weightsCopy.Count - 1)
                    .ToList()
                    .ForEach(i => cumSum[i] = cumSum[i - 1] + weightsCopy[i]);

                var factory = this.Numbers.Doubles(0, cumSum.Last());
                return () =>
                {
                    var r = factory();
                    for (int i = 0; i < cumSum.Length; i++)
                    {
                        if (cumSum[i] > r)
                            return copy[i];
                    }
                    return copy.Last();
                };
            }
        }

        public Func<string> Countries()
        {
            var data = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(culture => new RegionInfo(culture.LCID).EnglishName)
                .Distinct();

            return this.Items(data);
        }
    }
}