﻿using Newtonsoft.Json;
using NUnit.Framework.Constraints;
using System;

#if NET20
namespace ElasticElmah.Appender.net20.Tests
#else
namespace ElasticElmah.Appender.Tests
#endif

{
    class Json 
    {
        internal static IResolveConstraint IsEqualTo(Object expected)
        {
            return new JsonEqualsConstraint(expected);
        }
    }
    public class JsonEqualsConstraint : EqualConstraint
    {

        public JsonEqualsConstraint(object expected)
            : base(ToJson(expected))
        {
        }
        private static object ToJson(object value) 
        {
            if (ReferenceEquals(value, null)) 
            { 
                return null;
            }
            if (value is string) 
            {
                return JsonConvert.SerializeObject(JsonConvert.DeserializeObject((string)value),Formatting.Indented);
            }
            return JsonConvert.SerializeObject(value, Formatting.Indented);
        }
        public override bool Matches(object actual)
        {
            return base.Matches(ToJson(actual));
        }
    }
}
