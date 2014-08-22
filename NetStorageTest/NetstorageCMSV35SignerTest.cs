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
    public class NetstorageCMSv35SignerTest
    {
        [TestMethod]
        public void TestAPIParamsFormatter()
        {
            Assert.AreEqual(NetstorageCMSv35Signer.ParamsNameFormatter("ASDF"), "asdf");
            Assert.AreEqual(NetstorageCMSv35Signer.ParamsValueFormatter("ASDF"), "ASDF");
            Assert.AreEqual(NetstorageCMSv35Signer.ParamsValueFormatter("Lorem ipsum".ToByteArray()), "4c6f72656d20697073756d");
            Assert.AreEqual(NetstorageCMSv35Signer.ParamsValueFormatter(new DateTime(2013, 11, 11, 0, 0, 0, DateTimeKind.Utc)), "1384128000");
            Assert.AreEqual(NetstorageCMSv35Signer.ParamsValueFormatter(true), "1");
            Assert.AreEqual(NetstorageCMSv35Signer.ParamsValueFormatter(false), "0");
            Assert.AreEqual(NetstorageCMSv35Signer.ParamsValueFormatter(1234), "1234");
            Assert.AreEqual(NetstorageCMSv35Signer.ParamsValueFormatter(null as object), string.Empty);
        }

        [TestMethod]
        public void TestAPIActionParams()
        {
            var apiEvent = new APIParams()
            {
                Action = "download",
                Format = "xml",
                QuickDelete = "imreallyreallysure",
                Destination = "/foo",
                Target = "/bar",
                MTime = new DateTime(2013, 11, 11, 0, 0, 0, DateTimeKind.Utc),
                Size = 123,
                IndexZip = true
            };

            Assert.AreEqual(apiEvent.ConvertToQueryString(NetstorageCMSv35Signer.ParamsNameFormatter, NetstorageCMSv35Signer.ParamsValueFormatter), 
                "version=1&action=download&format=xml&quick-delete=imreallyreallysure&destination=%2Ffoo&target=%2Fbar&mtime=1384128000&size=123&index-zip=1");

            apiEvent = new APIParams()
            {
                MD5 = "Lorem ipsum".ToByteArray(),
                SHA1 = "Lorem ipsum".ToByteArray(),
                SHA256 = "Lorem ipsum".ToByteArray()
            };
            Assert.AreEqual(apiEvent.ConvertToQueryString(NetstorageCMSv35Signer.ParamsNameFormatter, NetstorageCMSv35Signer.ParamsValueFormatter),
                "version=1&md5=4c6f72656d20697073756d&sha1=4c6f72656d20697073756d&sha256=4c6f72656d20697073756d");
        }

        public NetstorageCMSv35Signer createNetStorageSigner()
        {
            var actionParams = new APIParams()
            {
                Action = "download"
            };

            return new NetstorageCMSv35Signer("GET", new Uri("http://www.example.com/foobar"), "user1", "secret1", actionParams);
        }

        [TestMethod]
        public void TestAPIActionHeaderValues()
        {
            var apiConnection = createNetStorageSigner();

            string actionHeader = apiConnection.GetActionHeaderValue();
            Assert.AreEqual(actionHeader, "version=1&action=download");
            string authDataHeader = apiConnection.GetAuthDataHeaderValue();
            
            Assert.IsTrue(Regex.IsMatch(authDataHeader, @"5, 0.0.0.0, 0.0.0.0, \d+, \d+, user1"));

            Assert.AreEqual(
                apiConnection.GetAuthSignHeaderValue("version=1&action=download", "5, 0.0.0.0, 0.0.0.0, 1384128000, 1234, user1"),
                "jKA6Rh9lCotwbE6BRPZve1fOl67yqKnZ+Z0b048jwYo=");
        }

        [TestMethod]
        public void TestAPIActionHeaders()
        {
            var apiConnection = createNetStorageSigner();
            WebHeaderCollection headers = apiConnection.ComputeHeaders();
            Assert.AreEqual(headers.Count, 4);
            Assert.IsNotNull(headers["X-Akamai-ACS-Action"]);
            Assert.IsNotNull(headers["X-Akamai-ACS-Auth-Data"]);
            Assert.IsNotNull(headers["X-Akamai-ACS-Auth-Sign"]);
            Assert.IsNotNull(headers["X-Akamai-NSKit"]);
        }
        [TestMethod]
        public void TestAPIActionValidateOK()
        {
            string TestURIProtocol = "asdf";
            WebRequest.RegisterPrefix(TestURIProtocol, new WebRequestTestCreate());
            var request = (HttpWebRequestTest)WebRequest.Create("asdf://www.example.com/");


            var nsSigner = createNetStorageSigner();
            var response = request.CreateResponse();
            nsSigner.Validate(response);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public void TestAPIActionValidateUnavailable()
        {
            string TestURIProtocol = "asdf";
            WebRequest.RegisterPrefix(TestURIProtocol, new WebRequestTestCreate());
            var request = (HttpWebRequestTest) WebRequest.Create("asdf://www.example.com/");


            var nsSigner = createNetStorageSigner();
            var response = request.CreateResponse(HttpStatusCode.ServiceUnavailable, "Server Unavailable");

            var currentDate = DateTime.UtcNow;
            var headers = new WebHeaderCollection { { "Date", currentDate.ToString("r") } };
            response = request.CreateResponse(HttpStatusCode.ServiceUnavailable, "Server Unavailable", headers);
            nsSigner.Validate(response);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public void TestAPIActionValidateDateDrift()
        {
            string TestURIProtocol = "asdf";
            WebRequest.RegisterPrefix(TestURIProtocol, new WebRequestTestCreate());
            var request = (HttpWebRequestTest)WebRequest.Create("asdf://www.example.com/");


            var nsSigner = createNetStorageSigner();
            var response = request.CreateResponse(HttpStatusCode.ServiceUnavailable, "Server Unavailable");

            var currentDate = DateTime.UtcNow.AddMinutes(-2);
            var headers = new WebHeaderCollection { { "Date", currentDate.ToString("r") } };
            response = request.CreateResponse(HttpStatusCode.ServiceUnavailable, "Server Unavailable", headers);
            try
            {
                nsSigner.Validate(response);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "Local server Date is more than 30s out of sync with Remote server");
                throw ex;
            }
        }

        [TestMethod]
        public void TestAPIActionExecute()
        {
            var nsSigner = createNetStorageSigner();
            string TestURIProtocol = "asdf";
            WebRequest.RegisterPrefix(TestURIProtocol, new WebRequestTestCreate());
            var request = (HttpWebRequestTest) WebRequest.Create("asdf://www.example.com/");

            var response = request.CreateResponse(HttpStatusCode.OK);
            request.NextResponse = response;

            Assert.AreSame(nsSigner.Execute(request), response.GetResponseStream()); 
            Assert.AreEqual(request.Method, "GET");
            Assert.AreEqual(request.Headers.Count, 4);

            nsSigner.Method = "PUT";
            nsSigner.Execute(request);
            Assert.AreEqual(request.ContentLength, 0);

            nsSigner.Method = "POST";
            var data = "Lorem ipsum dolor sit amet, an sea putant quaeque, homero aperiam te eos.".ToByteArray();
            nsSigner.UploadStream = new MemoryStream(data);
            nsSigner.Execute(request);
            Assert.AreEqual(request.ContentLength, 73);
            CollectionAssert.AreEqual(request.RequestStream.ToArray(), data);
        }

        [TestMethod]
        public void TestNetstorageGetUri()
        {
            var ns = new NetStorage("www.example.com", "user1", "secret1", false);
            Assert.AreEqual(ns.getNetStorageUri("/foobar"), "http://www.example.com/foobar");
        
            ns = new NetStorage("www.example.com", "user1", "secret1", true);
            Assert.AreEqual(ns.getNetStorageUri("/foobar"), "https://www.example.com/foobar");
        }

    }
}
