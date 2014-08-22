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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akamai.NetStorage;

namespace Akamai.NetStorage
{
    /// <summary>
    /// Command Line sample application to demonstrate the utilization of the NetstorageKit. 
    /// This can be used for both command line invocation or reference on how to leverage the 
    /// Kit. All supported commands are implemented in this sample for convience.
    /// 
    /// Author: colinb@akamai.com  (Colin Bendell)
    /// </summary>
    class CMS
    {
        static void Main(string[] args)
        {
            string action = null;
            string user = null;
            string key = null;
            string netstorageURI = null;
            string uploadfile = null;
            string outputfile = null;
            string targetFilename = null;
            string dstFilename = null;
            bool indexZip = false;
            

            string firstarg = null;
            foreach (string arg in args) 
            {
                if (firstarg != null)
                {
                    switch (firstarg) 
                    {
                        case "-h": 
                            help();
                            return;
                        case "-a":
                            action = arg;
                            break;
                        case "-u":
                            user = arg;
                            break;
                        case "-k":
                            key = arg;
                            break;
                        case "-o":
                            outputfile = arg;
                            break;
                        case "-f":
                            uploadfile = arg;
                            break;
                        case "-t":
                            targetFilename = arg;
                            break;
                        case "-d":
                            dstFilename = arg;
                            break;
                    }
                    firstarg = null;
                }
                else if (arg == "-indexzip")
                    indexZip = true;
                else if (!arg.StartsWith("-"))
                    netstorageURI = arg;
                else
                    firstarg = arg;
            }
            execute(action, user, key, netstorageURI, uploadfile, outputfile, targetFilename, dstFilename, indexZip);
        }

        static void execute(string action, string user, string key, string netstorageURI,
            string uploadfile, string outputfile, string target, string dst, bool indexZip)
        {
            if (action == null || netstorageURI == null || user == null || key == null)
            {
                help();
                return;
            }

            string[] hostpath = netstorageURI.Split("/".ToCharArray(), 2);
            string host = hostpath[0];
            string path = hostpath[1];
            NetStorage ns = new NetStorage(host, user, key);
            Stream result = null;
            bool success = true;

            switch (action)
            {
                case " delete":
                case "dir":
                    result = ns.Dir(path);
                    break;
                case "download":
                    result = ns.Download(path);
                    break;
                case "du":
                    result = ns.DU(path);
                    break;
                case "mkdir":
                    success = ns.MkDir(path);
                    break;
                case "mtime":
                    success = ns.MTime(path);
                    break;
                case "rename":
                    if (dst == null)
                    {
                        help();
                        return;
                    }
                    success = ns.Rename(path, dst);
                    break;
                case "rmdir":
                    success = ns.RmDir(path);
                    break;
                case "stat":
                    result = ns.Stat(path);
                    break;
                case "symlink":
                    if (target == null)
                    {
                        help();
                        return;
                    }
                    success = ns.Symlink(path, target);
                    break;
                case "upload":
                    if (uploadfile == null)
                    {
                        help();
                        return;
                    }
                    success = ns.Upload(path, new FileInfo(uploadfile), indexZip);
                    break;
                default:
                    help();
                    return;
            }

            if (result != null)
            {
                Stream output = Console.OpenStandardOutput();
                if (outputfile != null)
                    output = new FileInfo(outputfile).OpenWrite();
              
                using (output)
                {
                    using (result)
                    {
                        byte[] buffer = new byte[32*1024];
                        int bytesRead = 0;

                        while ((bytesRead = result.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            output.Write(buffer, 0, bytesRead);
                        }
                    }
                }
            }
            else if (success)
                Console.Out.WriteLine("Success.");
            else
                Console.Error.WriteLine("Error.");
        }

        static void help() 
        {
            Console.Error.WriteLine(@"
Usage: cms <-a action> <-u user> <-k key>
           [-o outfile] [-f srcfile]
           [-t targetpath] [-d originalpath]
           <-indexzip> <host+path>

Where:
    action          one of: delete, dir, download, du, mkdir, mtime, 
                    rename, rmdir, stat, symlink, upload 
    user            username defined in the Luna portal
    key             unique key used to sign api requests
    outfile         local file name to write when action=download
    srcfile         local file used as source when action=upload
    targetpath      the absolute path (/1234/example.jpg) pointing to the 
                    existing target when action=symlink
    originalpath    the absolute path (/1234/example.jpt) pointing to the 
                    original target when action=rename
    host+path       the netstorage hostname and path to the file being 
                    manipulated (example.akamaihd.net/1234/example.jpg)           

");
        }
    }
}
