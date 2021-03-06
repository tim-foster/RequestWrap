﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RequestWrap;

namespace RequestWrapTest
{
    [TestClass]
    public class basicUnitTests
    {
        public enum API
        {
            Testing,
            Production
        };

        public enum Method
        {
            Get,
            Queue,
            FollowerCount
        };

        [TestMethod]
        public void getNullTest()
        {
            IRequestWrapper a = new APIwrapper("http://www.testURL.com");
            a.addPreRequest(addHeaders)
                .setTimeout(20000)
                .setNullHandler()
                .endPointValidator((b) => b.Contains("bloop"));

            var result = a.get("bloop");
            Console.WriteLine(result.Result);

            a.setMessageHandler(new TestHandler()).clearEndPointValidator();
            Console.WriteLine(a.get("bleep").Result);
        }

        [TestMethod]
        public void invalidEndpoint()
        {
            IRequestWrapper a = new APIwrapper("http://www.testURL.com");
            a.addPreRequest(addHeaders)
                .setTimeout(20000)
                .setNullHandler()
                .endPointValidator((b) => b.Contains("blah"));

            try
            {
                var result = a.get("bloop").Result;
            }
            catch (AggregateException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void addHeaders(IRequestWrapper v)
        {
            v.HTTPclient.DefaultRequestHeaders.Clear();
            v.HTTPclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            v.HTTPclient.DefaultRequestHeaders.Add("X-WSSE", "blah");
        }


        [TestMethod]
        public void scheduleGetException()
        {
            IRequestWrapper a = new APIwrapper("http://www.testURL.com");
            a.setNullHandler();
            try
            {
                a.scheduleGet("blah");
            }
            catch (NotImplementedException e)
            {
                Assert.AreEqual("schedule get not implemented", e.Message);
            }
        }

        [TestMethod]
        public void executeTasksException()
        {
            IRequestWrapper a = new APIwrapper("http://www.testURL.com");
            a.setNullHandler().clearEndPointModifier();
            
            try
            {
                a.executeTasks();
            }
            catch (NotImplementedException e)
            {
                Assert.AreEqual("execute tasks not implemented", e.Message);
            }
        }
    }

    internal class TestHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(request.ToString())
            };

            Console.WriteLine("bleeep");

            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);
            return tsc.Task;
        }
    }

    [TestClass]
    public class CensusDataTests
    {
        delegate string endPointModifier(string EndPointURI);

        [TestMethod]
        public void censusTestOne()
        {
            string startupPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            var key = File.ReadAllText($"{startupPath}\\censusAPIkey.txt");
            var fullURL = "http://api.census.gov/data/2015/acs1?get=NAME,B01001_001E&for=state:*&key=..";
            IRequestWrapper a = new APIwrapper("http://api.census.gov");

            endPointModifier p = EndPointURI => string.Format("{EndPointURI}&key={key}");
            

            a.resetHandler().clearPreRequest().clearEndPointValidator();
            a.endPointModifier(p.Invoke);
            var result = a.get("data/2015/acs1?get=NAME,B01001_001E&for=state:*");
            Console.WriteLine(result.Result);
        }

    }

    [TestClass]
    public class basicPostTest
    {
        delegate string endPointModifier(string EndPointURI);

        [TestMethod]
        public void postTest()
        {
            string startupPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            var key = File.ReadAllText($"{startupPath}\\censusAPIkey.txt");
            IRequestWrapper a = new APIwrapper("http://api.census.gov");

            endPointModifier p = EndPointURI => string.Format("{EndPointURI}&key={key}");


            a.resetHandler().clearPreRequest().clearEndPointValidator().setNullHandler();
            a.endPointModifier(p.Invoke);
            var result = a.post("data/2015/acs1?get=NAME,B01001_001E&for=state:*", "");
            Console.WriteLine(result.Result);
        }

    }
}
