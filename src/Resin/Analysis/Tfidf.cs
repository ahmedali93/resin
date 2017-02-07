﻿using System;
using Resin.Querying;

namespace Resin.Analysis
{
    public class Tfidf : IScoringScheme
    {
        private readonly double _idf;

        public double Idf
        {
            get { return _idf; }
        }

        /// <summary>
        /// Create scoring scheme
        /// </summary>
        public Tfidf()
        {
        }

        /// <summary>
        /// Create scorer
        /// </summary>
        /// <param name="docsInCorpus"></param>
        /// <param name="docsWithTerm"></param>
        public Tfidf(int docsInCorpus, int docsWithTerm)
        {
            _idf = Math.Log(docsInCorpus / (double)docsWithTerm + 1) + 1;
        }

        public void Score(DocumentScore doc)
        {
            var tf = Math.Sqrt(doc.TermCount);
            doc.Score = tf * _idf;
        }

        public IScoringScheme CreateScorer(int docsInCorpus, int docsWithTerm)
        {
            return new Tfidf(docsInCorpus, docsWithTerm);
        }
    }
}