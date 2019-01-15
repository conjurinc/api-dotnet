﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using NUnit.Framework;
using static Conjur.Test.WebMocker;

namespace Conjur.Test
{
    public class VariablesTest : Base
    {
        public VariablesTest()
        {
            Client.Authenticator = new MockAuthenticator();
        }

        [Test]
        public void GetVariableTest()
        {
            Mocker.Mock(new Uri("test:///secrets/" + TestAccount + "/variable/foo%2Fbar"), "testvalue");
            Assert.AreEqual("testvalue", Client.Variable("foo/bar").GetValue());

            // TODO: not sure if this is supposed to be a plus or %20 or either
            // since we are using EscapeDataString %20 is convert to space, however plus is not converted anymore,
            Mocker.Mock(new Uri("test:///secrets/" + TestAccount + "/variable/foo%20bar"), "space test");
        }

        [Test]
        public void AddSecretTest()
        {
            char[] testValue = { 't', 'e', 's', 't', 'V', 'a', 'l', 'u', 'e' };
            var v = Mocker.Mock(new Uri("test:///secrets/" + TestAccount + "/variable/foobar"), "");
            v.Verifier = (WebRequest wr) => 
            {
                MockRequest req = wr as WebMocker.MockRequest;
                Assert.AreEqual(WebRequestMethods.Http.Post, wr.Method);
                Assert.AreEqual("text\\plain", wr.ContentType);
                Assert.AreEqual(testValue, req.Body);
            };
            Client.Variable("foobar").AddSecret(Encoding.UTF8.GetBytes(testValue));
        }

        [Test]
        public void CountVariablesTest()
        {
            string variableUri = $"test:///resources/{TestAccount}/{Constants.KIND_VARIABLE}";

            ClearMocker();
            Mocker.Mock(new Uri(variableUri + "?count=true&search=dummy"), @"{""count"":10}");

            UInt32 result = Client.CountVariables("dummy");
            Assert.AreEqual(result, 10);
        }

        [Test]
        public void ListVariableTest()
        {
            string variableUri = $"test:///resources/{TestAccount}/{Constants.KIND_VARIABLE}";
            IEnumerator<Variable> vars;

            ClearMocker();
            Mocker.Mock(new Uri(variableUri + "?offset=0&limit=1000"), GenerateVariablesInfo(0, 1000));
            Mocker.Mock(new Uri(variableUri + "?offset=1000&limit=1000"), GenerateVariablesInfo(1000, 2000));
            Mocker.Mock(new Uri(variableUri + "?offset=2000&limit=1000"), "[]");
            vars = (Client.ListVariables()).GetEnumerator();
            verifyVariablesInfo(vars, 2000);

            ClearMocker();
            Mocker.Mock(new Uri(variableUri + "?offset=0&limit=1000"), @"[""id"":""invalidjson""]");
            vars = (Client.ListVariables()).GetEnumerator();
            Assert.Throws<SerializationException>(() => vars.MoveNext());

        }

        private void verifyVariablesInfo(IEnumerator<Variable> vars, int excpectedNumVars)
        {
            for (int id = 0; id < excpectedNumVars; ++id)
            {
                Assert.AreEqual(true, vars.MoveNext());
                Assert.AreEqual($"{Client.GetAccountName()}:{Constants.KIND_VARIABLE}:id{id}", vars.Current.Id);
            }
            Assert.AreEqual(false, vars.MoveNext());
        }

        private string GenerateVariablesInfo(int firstVarId, int lastVarId)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int varId = firstVarId; varId < lastVarId; varId++)
            {
                stringBuilder.Append($"{{\"id\":\"{Client.GetAccountName()}:{Constants.KIND_VARIABLE}:id{varId}\"}},");
            }
            if (stringBuilder.Length != 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            return $"[{stringBuilder}]";
        }
    }
}
