// Copyright 2014 Akamai Technologies http://developer.akamai.com.
//
// Licensed under the Apache License, KitVersion 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Author: colinb@akamai.com  (Colin Bendell)
//

using Akamai.Netstorage;
using Akamai.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Akamai.NetStorage
{
    /// <summary>
    /// The Netstorage class is the preferred interface for calling libraries indending to leverage the Netstorage API. 
    /// All of the available actions are innumerated in this library and are responsible for the correct business
    /// logic to assemble the request to the API. Some early safetys are added in this library to limit errors.
    /// 
    /// TODO: Add "LIST" support for ObjectStore
    /// TODO: Detect FileStore v. ObjectStore
    /// TODO: Extract xml response from various requests into standard object representation
    /// 
    /// Author: colinb@akamai.com  (Colin Bendell)
    /// </summary>
    public class NetStorage
    {
        public string HostName { get; private set; }
        public string Username { get; private set; }
        public string Key { get; private set; }
        public bool UseSSL { get; private set; }
        public WebRequest Request { get; set; }

        public NetStorage(string hostname, string username, string key, bool useSSL = false)
        {
            this.HostName = hostname;
            this.Username = username;
            this.Key = key;
            this.UseSSL = useSSL;
        }

        internal Uri getNetStorageUri(string path)
        {
            return new UriBuilder() { Scheme = this.UseSSL ? "HTTPS" : "HTTP", Host = this.HostName, Path = path }.Uri;
        }

        internal Stream execute(string method, string path, APIParams acsParams, Stream uploadStream = null) 
        {
            return new NetstorageCMSv35Signer(
                method, 
                this.getNetStorageUri(path), 
                this.Username, 
                this.Key,
                acsParams,
                uploadStream).Execute(this.Request);
        }

        public bool Delete(string path)
        {
            using (execute("POST", path, new APIParams() { Action = "delete" })) { }
            return true;
        }

        public Stream Dir(string path, string format = "xml")
        {
            //TODO: strip final slash on dir command
            return execute("GET", path, new APIParams() { Action = "dir", Format = format });
        }

        public Stream Download(string path)
        {
            return execute("GET", path, new APIParams() { Action = "download" });
        }

        public Stream DU(string path, string format = "xml")
        {
            return execute("GET", path, new APIParams() { Action = "du", Format = format });
        }

        public bool MkDir(string path)
        {
            using (execute("PUT", path, new APIParams() { Action = "mkdir" })) { }
            return true;
        }

        public bool MTime(string path, DateTime? mTime = null)
        {
            //TODO: verify that this is for a file - cannot mtime on symlinks or dirs
            mTime = mTime ?? DateTime.Now;
            using (execute("PUT", path, new APIParams() { Action = "mtime", MTime = mTime })) { }
            return true;
        }

        public bool Rename(string path, string original)
        {
            //TODO: validate path and destination start with the same cpcode
            using (execute("PUT", path, new APIParams() { Action = "rename", Destination = original })) { }
            return true;
        }

        public bool RmDir(string path)
        {
            using (execute("POST", path, new APIParams() { Action = "rmdir" })) { }
            return true;
        }

        public Stream Stat(string path, string format = "xml")
        {
            return execute("GET", path, new APIParams() { Action = "stat", Format = format });
        }

        public bool Symlink(string path, string target)
        {
            using (execute("PUT", path, new APIParams() { Action = "symlink", Target = target })) { }
            return true;
        }

        public bool QuickDelete(string path)
        {
            using (execute("PUT", path, new APIParams() { Action = "quick-delete", QuickDelete = "imreallyreallysure" })) { }
            return true;
        }

        public bool Upload(string path, Stream uploadFileStream, DateTime? mTime = null, long? size = null, byte[] md5Checksum = null, byte[] sha1Checksum = null, byte[] sha256Checksum = null, bool? indexZip = null)
        {
            var acsParams = new APIParams()
            {
                Action = "upload",
                MTime = mTime,
                Size = size,
                MD5 = md5Checksum,
                SHA1 = sha1Checksum,
                SHA256 = sha256Checksum,
                IndexZip = (indexZip == true) ? indexZip : null
            };

            // sanity check to ensure that indexZip is only true if the file destination is also a zip.
            // probably should throw an exception or warning instead.
            if (acsParams.IndexZip == true && !path.EndsWith(".zip"))
                acsParams.IndexZip = null;

            // size is not supported with zip since the index-zip funtionality mutates the file thus inconsistency on which size value to use
            // probably should throw an exception or a warning
            if (acsParams.Size != null && acsParams.IndexZip == true)
                acsParams.Size = null;

            execute("PUT", path, acsParams, uploadFileStream);
            return true;
        }

        public bool Upload(string path, FileInfo srcFile, bool? indexZip = null)
        {
            if (!srcFile.Exists) throw new FileNotFoundException("Src file is not accessible", srcFile.ToString());

            DateTime mTime = srcFile.LastWriteTime;
            byte[] checksum = null;
            Stream stream;
            using (stream = new BufferedStream(srcFile.OpenRead(), 1024*1024))
            {
                checksum = stream.ComputeHash(NetstorageCMSv35Signer.HashType.SHA256.Checksum);
            }

            stream = srcFile.OpenRead();
            long size = srcFile.Length;

            return this.Upload(path, stream, mTime, size, sha256Checksum: checksum, indexZip: indexZip);
        }
    }
}
