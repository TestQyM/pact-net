﻿using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PactNet.Comparers;

namespace PactNet.Mocks.MockHttpService.Comparers
{
    public class HttpBodyComparer : IHttpBodyComparer
    {
        private readonly string _messagePrefix;

        public HttpBodyComparer(string messagePrefix)
        {
            _messagePrefix = messagePrefix;
        }

        //TODO: Remove boolean and add "matching" functionality
        public ComparisonResult Compare(dynamic expected, dynamic actual, bool useStrict = false)
        {
            var result = new ComparisonResult();

            if (expected == null)
            {
                return result;
            }

            if (expected != null && actual == null)
            {
                result.AddError("Body is null");
                return result;
            }

            //TODO: Maybe look at changing these to JToken.FromObject(...)
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            var expectedToken = JsonConvert.DeserializeObject<JToken>(expectedJson);
            var actualToken = JsonConvert.DeserializeObject<JToken>(actualJson);

            if (useStrict)
            {
                if (!JToken.DeepEquals(expectedToken, actualToken))
                {
                    result.AddError(expected: expectedToken, actual: actualToken);
                }
                return result;
            }

            AssertPropertyValuesMatch(expectedToken, actualToken, result);

            return result;
        }

        private bool AssertPropertyValuesMatch(JToken httpBody1, JToken httpBody2, ComparisonResult result)
        {
            switch (httpBody1.Type)
            {
                case JTokenType.Array: 
                    {
                        if (httpBody1.Count() != httpBody2.Count())
                        {
                            result.AddError(expected: httpBody1.Root, actual: httpBody2.Root);
                            return false;
                        }

                        for (var i = 0; i < httpBody1.Count(); i++)
                        {
                            if (httpBody2.Count() > i)
                            {
                                var isMatch = AssertPropertyValuesMatch(httpBody1[i], httpBody2[i], result);
                                if (!isMatch)
                                {
                                    break;
                                }
                            }
                        }
                        break;
                    }
                case JTokenType.Object:
                    {
                        foreach (JProperty item1 in httpBody1)
                        {
                            var item2 = httpBody2.Cast<JProperty>().SingleOrDefault(x => x.Name == item1.Name);

                            if (item2 != null)
                            {
                                var isMatch = AssertPropertyValuesMatch(item1, item2, result);
                                if (!isMatch)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                result.AddError(expected: httpBody1.Root, actual: httpBody2.Root);
                                return false;
                            }
                        }
                        break;
                    }
                case JTokenType.Property: 
                    {
                        var httpBody2Item = httpBody2.SingleOrDefault();
                        var httpBody1Item = httpBody1.SingleOrDefault();

                        if (httpBody2Item == null && httpBody1Item == null)
                        {
                            return true;
                        }

                        if (httpBody2Item != null && httpBody1Item != null)
                        {
                            AssertPropertyValuesMatch(httpBody1Item, httpBody2Item, result);
                        }
                        else
                        {
                            result.AddError(expected: httpBody1.Root, actual: httpBody2.Root);
                            return false;
                        }
                        break;
                    }
                case JTokenType.Integer:
                case JTokenType.String: 
                    {
                        if (!httpBody1.Equals(httpBody2))
                        {
                            result.AddError(expected: httpBody1.Root, actual: httpBody2.Root);
                            return false;
                        }
                        break;
                    }
                default:
                    {
                        if (!JToken.DeepEquals(httpBody1, httpBody2))
                        {
                            result.AddError(expected: httpBody1.Root, actual: httpBody2.Root);
                            return false;
                        }
                        break;
                    }
            }

            return true;
        }
    }
}