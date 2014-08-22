// Copyright 2014 Akamai Technologies http://developer.akamai.com.
//
// Licensed under the Apache License, Version 2.0 (the "License");
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

using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akamai.Utils;
using Akamai.Netstorage;
using Akamai.NetStorageTest;

namespace Akamai.NetStorage
{
    [TestClass]
    public class NetstorageTest
    {

        public NetStorage SetupNetstorage(out WebHeaderCollection headers, out MemoryStream requestStream)
        {
            string TestURIProtocol = "asdf";
            WebRequest.RegisterPrefix(TestURIProtocol, new WebRequestTestCreate());
            var request = (HttpWebRequestTest)WebRequest.Create("asdf://www.example.com/");
            var response = request.CreateResponse(HttpStatusCode.OK);
            request.NextResponse = response;

            headers = request.Headers;
            requestStream = request.RequestStream;

            NetStorage ns = new NetStorage("www.example.com", "user1", "secret1", false);
            ns.Request = request;

            return ns;
        }

        [TestMethod]
        public void TestNetstorageDelete()
        {
            WebHeaderCollection headers;
            MemoryStream requestStream;

            var ns = SetupNetstorage(out headers, out requestStream);
            ns.Delete("/foobar");
            Assert.AreEqual(headers.Count, 4);
            Assert.AreEqual(headers["X-Akamai-ACS-Action"], "version=1&action=delete");
        }

        [TestMethod]
        public void TestNetstorageDir()
        {
            WebHeaderCollection headers;
            MemoryStream requestStream;

            var ns = SetupNetstorage(out headers, out requestStream);
            ns.Dir("/foobar");
            Assert.AreEqual(headers.Count, 4);
            Assert.AreEqual(headers["X-Akamai-ACS-Action"], "version=1&action=dir&format=xml");
        }

        [TestMethod]
        public void TestNetstorageDU()
        {
            WebHeaderCollection headers;
            MemoryStream requestStream;

            var ns = SetupNetstorage(out headers, out requestStream);
            ns.DU("/foobar");
            Assert.AreEqual(headers.Count, 4);
            Assert.AreEqual(headers["X-Akamai-ACS-Action"], "version=1&action=du&format=xml");
        }

        [TestMethod]
        public void TestNetstorageDownload()
        {
            WebHeaderCollection headers;
            MemoryStream requestStream;

            var ns = SetupNetstorage(out headers, out requestStream);
            ns.Download("/foobar");
            Assert.AreEqual(headers.Count, 4);
            Assert.AreEqual(headers["X-Akamai-ACS-Action"], "version=1&action=download");
        }

        [TestMethod]
        public void TestNetstorageMkDir()
        {
            WebHeaderCollection headers;
            MemoryStream requestStream;

            var ns = SetupNetstorage(out headers, out requestStream);
            ns.MkDir("/foobar");
            Assert.AreEqual(headers.Count, 4);
            Assert.AreEqual(headers["X-Akamai-ACS-Action"], "version=1&action=mkdir");
        }

        [TestMethod]
        public void TestNetstorageMTime()
        {
            WebHeaderCollection headers;
            MemoryStream requestStream;

            var ns = SetupNetstorage(out headers, out requestStream);
            ns.MTime("/foobar", new DateTime(2013, 11, 11, 0, 0, 0));
            Assert.AreEqual(headers.Count, 4);
            Assert.AreEqual(headers["X-Akamai-ACS-Action"], "version=1&action=mtime&mtime=1384128000");

            ns = SetupNetstorage(out headers, out requestStream);
            ns.MTime("/foobar");
            
            Assert.IsTrue(Regex.IsMatch(headers["X-Akamai-ACS-Action"], @"version=1&action=mtime&mtime=\d+"));
        }

        [TestMethod]
        public void TestNetstorageRename()
        {
            //TODO: test if the destination already exists
            //TODO: test that the prefix cpcode is the same
            WebHeaderCollection headers;
            MemoryStream requestStream;

            var ns = SetupNetstorage(out headers, out requestStream);
            ns.Rename("/foobar", "/barfoo");

            Assert.AreEqual(headers.Count, 4);
            Assert.AreEqual(headers["X-Akamai-ACS-Action"], "version=1&action=rename&destination=%2Fbarfoo");
        }

        [TestMethod]
        public void TestNetstorageRmDir()
        {
            WebHeaderCollection headers;
            MemoryStream requestStream;

            var ns = SetupNetstorage(out headers, out requestStream);
            ns.RmDir("/foobar");

            Assert.AreEqual(headers.Count, 4);
            Assert.AreEqual(headers["X-Akamai-ACS-Action"], "version=1&action=rmdir");
        }

        [TestMethod]
        public void TestNetstorageStat()
        {
            WebHeaderCollection headers;
            MemoryStream requestStream;

            var ns = SetupNetstorage(out headers, out requestStream);
            ns.Stat("/foobar");

            Assert.AreEqual(headers.Count, 4);
            Assert.AreEqual(headers["X-Akamai-ACS-Action"], "version=1&action=stat&format=xml");
        }

        [TestMethod]
        public void TestNetstorageSymlink()
        {
            //TODO: test if the target exists
            WebHeaderCollection headers;
            MemoryStream requestStream;

            var ns = SetupNetstorage(out headers, out requestStream);
            ns.Symlink("/foobar", "/barfoo");

            Assert.AreEqual(headers.Count, 4);
            Assert.AreEqual(headers["X-Akamai-ACS-Action"], "version=1&action=symlink&target=%2Fbarfoo");
        }

        [TestMethod]
        public void TestNetstorageQuickDelete()
        {
            WebHeaderCollection headers;
            MemoryStream requestStream;

            var ns = SetupNetstorage(out headers, out requestStream);
            ns.QuickDelete("/foobar");

            Assert.AreEqual(headers.Count, 4);
            Assert.AreEqual(headers["X-Akamai-ACS-Action"], "version=1&action=quick-delete&quick-delete=imreallyreallysure");
        }

        [TestMethod]
        public void TestNetstorageUpload()
        {
            WebHeaderCollection headers;
            MemoryStream requestStream;

            var ns = SetupNetstorage(out headers, out requestStream);
            var data = "Lorem ipsum dolor sit amet, an sea putant quaeque, homero aperiam te eos.";
            var stream = new MemoryStream(data.ToByteArray());

            ns.Upload("/foobar", stream, new DateTime(2013, 11, 11, 0, 0, 0, DateTimeKind.Utc), 73, new byte[] { 0 }, new byte[] { 1 }, new byte[] { 2 }, false);

            Assert.AreEqual(headers.Count, 4);
            Assert.AreEqual(headers["X-Akamai-ACS-Action"], "version=1&action=upload&mtime=1384128000&size=73&md5=00&sha1=01&sha256=02");
            Assert.AreEqual(Encoding.UTF8.GetString(requestStream.ToArray()), data);

            ns = SetupNetstorage(out headers, out requestStream);
            stream = new MemoryStream(data.ToByteArray());
            ns.Upload("/foobar", stream, new DateTime(2013, 11, 11, 0, 0, 0, DateTimeKind.Utc), 73, indexZip: true);
            Assert.AreEqual(headers["X-Akamai-ACS-Action"], "version=1&action=upload&mtime=1384128000&size=73");

            ns = SetupNetstorage(out headers, out requestStream);
            stream = new MemoryStream(data.ToByteArray());
            ns.Upload("/foobar.zip", stream, new DateTime(2013, 11, 11, 0, 0, 0, DateTimeKind.Utc), 73, indexZip: true);
            Assert.AreEqual(headers["X-Akamai-ACS-Action"], "version=1&action=upload&mtime=1384128000&index-zip=1");

            ns = SetupNetstorage(out headers, out requestStream);
            stream = new MemoryStream(data.ToByteArray());
            ns.Upload("/foobar.zip", stream, new DateTime(2013, 11, 11, 0, 0, 0, DateTimeKind.Utc), indexZip: true);
            Assert.AreEqual(headers["X-Akamai-ACS-Action"], "version=1&action=upload&mtime=1384128000&index-zip=1");
        }

        [TestMethod]
        public void TestNetstorageUploadFile()
        {

            var data = "Lorem ipsum dolor sit amet, an sea putant quaeque, homero aperiam te eos.";
            string tmpFilename = Path.GetTempPath() + Guid.NewGuid().ToString() + ".txt";

            try
            {
                using (StreamWriter writer = File.CreateText(tmpFilename))
                {
                    writer.Write(data);
                }

                WebHeaderCollection headers;
                MemoryStream requestStream;
                var ns = SetupNetstorage(out headers, out requestStream);
                var file = new FileInfo(tmpFilename);
                file.LastWriteTime = new DateTime(2013, 11, 11, 0, 0, 0);

                ns.Upload("/foobar", file, false);


                Assert.AreEqual(headers.Count, 4);
                Assert.AreEqual(headers["X-Akamai-ACS-Action"], "version=1&action=upload&mtime=1384128000&size=73&sha256=4e8aecd6dc4c97ae55c30ef9b1e91b4829ef5871b16262b4628838a80dc0c2e2");
                Assert.IsTrue(Regex.IsMatch(headers["X-Akamai-ACS-Action"], @"version=1&action=upload&mtime=\d+&size=73&sha256=4e8aecd6dc4c97ae55c30ef9b1e91b4829ef5871b16262b4628838a80dc0c2e2"));
                Assert.AreEqual(Encoding.UTF8.GetString(requestStream.ToArray()), data);             
            }
            finally
            {
                if (File.Exists(tmpFilename))
                {
                    File.Delete(tmpFilename);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TestNetstorageUploadFileException()
        {
            string tmpFilename = Path.GetTempPath() + Guid.NewGuid().ToString() + ".txt"; // Does Not exist!

            WebHeaderCollection headers;
            MemoryStream requestStream;
            var ns = SetupNetstorage(out headers, out requestStream);
            ns.Upload("/foobar", new FileInfo(tmpFilename + ".junk"), false);
        }

    }
}
