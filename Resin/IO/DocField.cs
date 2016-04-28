﻿using System;
using System.Collections.Generic;

namespace Resin.IO
{
    //[Serializable]
    //public class DocField
    //{
    //    private readonly string _docId;
    //    private readonly string _value;
    //    private readonly string _field;

    //    public string Field
    //    {
    //        get { return _field; }
    //    }

    //    public string Value
    //    {
    //        get { return _value; }
    //    }

    //    public string DocId
    //    {
    //        get { return _docId; }
    //    }

    //    public DocField(string docId, string field, string value)
    //    {
    //        _docId = docId;
    //        _field = field;
    //        _value = value;
    //    }
    //}

    [Serializable]
    public class DocContainerFile : CompressedFileBase<DocContainerFile>
    {
        private readonly string _id;
        public string Id { get { return _id; } }

        private readonly Dictionary<string, Document> _files;
        /// <summary>
        /// docid/file
        /// </summary>
        public Dictionary<string, Document> Files { get { return _files; } }
        
        public DocContainerFile(string id)
        {
            _id = id;
            _files = new Dictionary<string, Document>();
        }
    }

    [Serializable]
    public class PostingsContainerFile : CompressedFileBase<PostingsContainerFile>
    {
        private readonly string _id;
        public string Id { get { return _id; } }

        private readonly Dictionary<string, PostingsFile> _files;
        /// <summary>
        /// field.token/file
        /// </summary>
        public Dictionary<string, PostingsFile> Files { get { return _files; } }
        public PostingsContainerFile(string id)
        {
            _id = id;
            _files = new Dictionary<string, PostingsFile>();
        }

        public PostingsFile Pop(string key)
        {
            var p = Files[key];
            Files.Remove(key);
            return p;
        }
    }
}